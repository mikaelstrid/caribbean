using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
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

namespace Aruba.PdfWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private QueueClient _client;
        private readonly ManualResetEvent _completedEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("Starting PdfPageGenerator worker role.");

            _client.OnMessageAsync(async receivedMessage =>
            {
                string messageBody = null;
                try
                {
                    messageBody = receivedMessage.GetBody<string>();
                    Trace.TraceInformation("Message received: {0}", messageBody);
                    var message = JsonConvert.DeserializeObject<PdfPageGeneratorQueueMessage>(messageBody);

                    var localStorage = RoleEnvironment.GetLocalResource("LocalStoreForTemporaryPdfs").RootPath;
                    var pageUrl = string.Format(RoleEnvironment.GetConfigurationSettingValue("RenderPageBaseUrl"), message.AgentUserId, message.PageId);
                    var nameWithoutExtension = $"{message.PageId}-{Guid.NewGuid().ToString("N")}";
                    var thumbnailName = nameWithoutExtension + ".jpg";
                    var pdfName = nameWithoutExtension + ".pdf";

                    // === THUMBNAIL ===
                    var thumbnailLocalFilePath = RenderThumbnail(pageUrl, thumbnailName, localStorage, message);
                    Trace.TraceInformation("Thumbnail created: {0}", thumbnailLocalFilePath);

                    var thumbnailBlob = UploadAssetToBlobStorage(thumbnailLocalFilePath, "pagethumbnails");
                    File.Delete(thumbnailLocalFilePath);
                    Trace.TraceInformation("Thumbnail uploaded: {0}", thumbnailBlob.Uri.ToString());

                    await CallServiceToSaveAssetReferenceInDatabase(message.AgentUserId, message.PageId, thumbnailBlob.Name, thumbnailBlob.Uri.ToString(), "UpdatePageThumbnailUrl");

                    // === PDF ===
                    var pdfLocalFilePath = RenderPdf(pageUrl, pdfName, localStorage, message);
                    Trace.TraceInformation("PDF created: {0}", pdfLocalFilePath);

                    var pdfBlob = UploadAssetToBlobStorage(pdfLocalFilePath, "pagepdfs");
                    File.Delete(pdfLocalFilePath);
                    Trace.TraceInformation("PDF uploaded: {0}", pdfBlob.Uri.ToString());

                    await CallServiceToSaveAssetReferenceInDatabase(message.AgentUserId, message.PageId, pdfBlob.Name, pdfBlob.Uri.ToString(), "UpdatePagePdfUrl");

                    receivedMessage.Complete();
                }
                catch (Exception e)
                {
                    if (string.IsNullOrWhiteSpace(messageBody))
                        Trace.TraceError("Error parsing message: {0}", receivedMessage.ToString());
                    else
                        Trace.TraceError("Error when processing message {0}: {1} ", messageBody, e.ToString());

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
                $"{"phantomjs-script-thumbnail.js"} {pageUrl} {outputFilePath} {message.PageWidth} {message.PageHeight} {message.ThumbnailWidth} {message.ThumbnailHeight}";

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

            process.OutputDataReceived += (s, a) => outputBuilder.Append(a.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit(20000);
            process.CancelOutputRead();

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

            process.OutputDataReceived += (s, a) => outputBuilder.Append(a.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit(20000);
            process.CancelOutputRead();

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

        private static async Task CallServiceToSaveAssetReferenceInDatabase(string agentUserId, int pageId, string assetName, string assetUrl, string urlConfigurationSettingName)
        {
            var updateAssetUrl = RoleEnvironment.GetConfigurationSettingValue(urlConfigurationSettingName);
            using (var client = new HttpClient())
            {
                var apiModel = new PostAssetApiModel { AgentUserId = agentUserId, PageId = pageId, AssetName = assetName, AssetUrl = assetUrl };
                var response = await client.PostAsJsonAsync(updateAssetUrl, apiModel);
            }
        }

        public override bool OnStart()
        {
            var queueName = RoleEnvironment.GetConfigurationSettingValue("Aruba.PdfWorkerQueueName");

            ServicePointManager.DefaultConnectionLimit = 12;
            var connectionString = RoleEnvironment.GetConfigurationSettingValue("Microsoft.ServiceBus.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            if (!namespaceManager.QueueExists(queueName)) namespaceManager.CreateQueue(queueName);

            _client = QueueClient.CreateFromConnectionString(connectionString, queueName);
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