using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caribbean.Aruba.SharedTypes;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using NLog;

namespace Aruba.PdfWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private const int DEFAULT_RENDER_TIMEOUT_IN_SECONDS = 120;

        private QueueClient _client;
        private readonly ManualResetEvent _completedEvent = new ManualResetEvent(false);

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public override void Run()
        {
            _client.OnMessageAsync(async receivedMessage =>
            {
                string messageBody = null;
                try
                {
                    messageBody = receivedMessage.GetBody<string>();
                    Logger.Debug("Message received: {0}", messageBody);
                    var message = JsonConvert.DeserializeObject<PdfPageGeneratorQueueMessage>(messageBody);

                    var localStorage = RoleEnvironment.GetLocalResource("LocalStoreForTemporaryPdfs").RootPath;
                    var pageUrl = string.Format(RoleEnvironment.GetConfigurationSettingValue("RenderPageBaseUrl"), message.AgentUserId, message.PageId);
                    var thumbnailName = $"{message.PageId}-{message.ThumbnailJobId.ToString("N")}" + ".jpg";
                    var pdfName = $"{message.PageId}-{message.PdfJobId.ToString("N")}" + ".pdf";

                    var stopWatch = new Stopwatch();

                    // === THUMBNAIL ===
                    stopWatch.Start();
                    var thumbnailLocalFilePath = RenderThumbnail(pageUrl, thumbnailName, localStorage, message);
                    var elapsed = stopWatch.ElapsedMilliseconds;
                    Logger.Debug($"Thumbnail created: {thumbnailLocalFilePath} in {elapsed} ms.");

                    var thumbnailBlob = UploadAssetToBlobStorage(thumbnailLocalFilePath, "pagethumbnails");
                    if (thumbnailBlob != null)
                    {
                        File.Delete(thumbnailLocalFilePath);
                        Logger.Debug($"Thumbnail uploaded: {thumbnailBlob.Uri.ToString()} in {stopWatch.ElapsedMilliseconds - elapsed} ms.");

                        await CallServiceToSaveAssetReferenceInDatabase(message.AgentUserId, message.PageId, message.ThumbnailJobId, stopWatch.ElapsedMilliseconds, thumbnailBlob.Name, thumbnailBlob.Uri.ToString(), "UpdatePageThumbnailUrl");
                    }
                    else
                        Logger.Warn("Thumbnail could not be uploaded.");

                    // === PDF ===
                    stopWatch.Restart();
                    var pdfLocalFilePath = RenderPdf(pageUrl, pdfName, localStorage, message);
                    elapsed = stopWatch.ElapsedMilliseconds;
                    Logger.Debug($"PDF created: {pdfLocalFilePath} in {elapsed} ms.");

                    var pdfBlob = UploadAssetToBlobStorage(pdfLocalFilePath, "pagepdfs");
                    if (pdfBlob != null)
                    {
                        File.Delete(pdfLocalFilePath);
                        Logger.Debug($"PDF uploaded: {pdfBlob.Uri.ToString()} in {stopWatch.ElapsedMilliseconds - elapsed} ms.");

                        await CallServiceToSaveAssetReferenceInDatabase(message.AgentUserId, message.PageId, message.PdfJobId, stopWatch.ElapsedMilliseconds, pdfBlob.Name, pdfBlob.Uri.ToString(), "UpdatePagePdfUrl");
                    }
                    else
                        Logger.Warn("PDF could not be uploaded.");

                    receivedMessage.Complete();
                    stopWatch.Stop();
                }
                catch (Exception e)
                {
                    if (string.IsNullOrWhiteSpace(messageBody))
                        Logger.Warn("Error parsing message (message body null or empty): {0}", receivedMessage.ToString());
                    else
                        Logger.Warn("Error when processing message {0}: {1} ", messageBody, e.ToString());

                    receivedMessage.DeadLetter("Exception when processing", e.ToString());
                }
            });
            _completedEvent.WaitOne();
        }

        private static string RenderThumbnail(string pageUrl, string thumbnailName, string targetDirectory, PdfPageGeneratorQueueMessage message)
        {
            var outputBuilder = new StringBuilder();
            var outputFilePath = Path.Combine(targetDirectory, thumbnailName);

            var args =
                $"--disk-cache=true {"phantomjs-script-thumbnail.js"} {pageUrl} {outputFilePath} {message.PageWidth} {message.PageHeight} {message.ThumbnailWidth} {message.ThumbnailHeight}";

            var processStartInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                Arguments = args,
                FileName = "phantomjs.exe"
            };

            var process = new Process
            {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true
            };

            int timeoutInSeconds;
            if (!int.TryParse(RoleEnvironment.GetConfigurationSettingValue("RenderThumbnailTimeoutInSeconds"), out timeoutInSeconds)) timeoutInSeconds = DEFAULT_RENDER_TIMEOUT_IN_SECONDS;
            
            process.OutputDataReceived += (s, a) => outputBuilder.Append(a.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit(timeoutInSeconds * 1000);
            process.CancelOutputRead();

            if (outputBuilder.Length > 0) Logger.Trace("Output from RenderThumbnail: " + outputBuilder);

            return outputFilePath;
        }
        private static string RenderPdf(string pageUrl, string thumbnailName, string targetDirectory, PdfPageGeneratorQueueMessage message)
        {
            var outputBuilder = new StringBuilder();
            var outputFilePath = Path.Combine(targetDirectory, thumbnailName);

            var args =
                $"{"phantomjs-script-pdf.js"} {pageUrl} {outputFilePath} {message.PageWidth} {message.PageHeight} {message.Dpi}";

            var processStartInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                Arguments = args,
                FileName = "phantomjs.exe"
            };

            var process = new Process
            {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true
            };

            //:#263: Increase the timeout since the job sometimes takes long time
            int timeoutInSeconds;
            if (!int.TryParse(RoleEnvironment.GetConfigurationSettingValue("RenderPdfTimeoutInSeconds"), out timeoutInSeconds)) timeoutInSeconds = DEFAULT_RENDER_TIMEOUT_IN_SECONDS;

            process.OutputDataReceived += (s, a) => outputBuilder.Append(a.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit(timeoutInSeconds * 1000);
            process.CancelOutputRead();

            if (outputBuilder.Length > 0) Logger.Trace("Output from RenderPdf: " + outputBuilder);

            return outputFilePath;
        }

        private static CloudBlockBlob UploadAssetToBlobStorage(string localFilePath, string containerName)
        {
            var container = GetContainer(containerName);
            if (container == null) return null;

            var blob = container.GetBlockBlobReference(Path.GetFileName(localFilePath));
            blob.UploadFromFile(localFilePath, FileMode.OpenOrCreate);

            return blob;
        }
        private static CloudBlobContainer GetContainer(string containerName)
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));
                var blobClient = storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(containerName);
                return container.Exists() ? container : null;
            }
            catch
            {
                return null;
            }
        }

        private static async Task CallServiceToSaveAssetReferenceInDatabase(string agentUserId, int pageId, Guid jobId, long jobDuration, string assetName, string assetUrl, string urlConfigurationSettingName)
        {
            var updateAssetUrl = RoleEnvironment.GetConfigurationSettingValue(urlConfigurationSettingName);
            using (var client = new HttpClient())
            {
                var apiModel = new PostAssetApiModel { AgentUserId = agentUserId, PageId = pageId, JobId = jobId, JobDurationMs = jobDuration, AssetName = assetName, AssetUrl = assetUrl };
                var response = await client.PostAsJsonAsync(updateAssetUrl, apiModel);
                Logger.Trace("Response from HttpClient: {0}, {1}", response.StatusCode, response.ReasonPhrase);
            }
        }

        public override bool OnStart()
        {
            LogTargetManager.SetLogTargetBaseDirectory("AzureLocalStorageFile", RoleEnvironment.GetLocalResource("CustomLogs").RootPath);

            var queueName = RoleEnvironment.GetConfigurationSettingValue("Aruba.PdfWorkerQueueName");

            ServicePointManager.DefaultConnectionLimit = 12;
            var connectionString = RoleEnvironment.GetConfigurationSettingValue("Microsoft.ServiceBus.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            if (!namespaceManager.QueueExists(queueName)) namespaceManager.CreateQueue(queueName);

            _client = QueueClient.CreateFromConnectionString(connectionString, queueName);

            Logger.Info("Starting Worker Role Instance (v{0})...", Assembly.GetExecutingAssembly().GetName().Version.ToString());

            return base.OnStart();
        }
        public override void OnStop()
        {
            _client.Close();
            _completedEvent.Set();
            base.OnStop();
        }
    }
}
