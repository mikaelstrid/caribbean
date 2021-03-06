using System;
using System.Threading.Tasks;
using System.Web.Configuration;
using Caribbean.Aruba.SharedTypes;
using Caribbean.Aruba.Web.Hubs;
using Caribbean.DataAccessLayer.Database;
using Caribbean.Models.Database;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using NLog;

namespace Caribbean.Aruba.Web.Business
{
    public interface IPagePdfGeneratorProxyService
    {
        void Initialize();
        void QueueJob(int pageId, string agentUserId, int templateWidth, int templateHeight, int dpi, int thumbnailWidth, int thumbnailHeight);
    }

    public class PagePdfGeneratorProxyService : IPagePdfGeneratorProxyService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationsBroadcaster _notificationsBroadcaster;
        private QueueClient _ordersQueueClient;

        public PagePdfGeneratorProxyService(IUnitOfWork unitOfWork, INotificationsBroadcaster notificationsBroadcaster)
        {
            _unitOfWork = unitOfWork;
            _notificationsBroadcaster = notificationsBroadcaster;
        }

        public void Initialize()
        {
            if (_ordersQueueClient != null) return;
            _ordersQueueClient = QueueClient.CreateFromConnectionString(
                WebConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"],
                WebConfigurationManager.AppSettings["Aruba.PdfWorkerQueueName"]);
        }

        public async void QueueJob(int pageId, string agentUserId, int templateWidth, int templateHeight, int dpi, int thumbnailWidth, int thumbnailHeight)
        {
            try
            {
                var page = await _unitOfWork.PageRepository.GetById(pageId);
                if (page == null)
                {
                    Logger.Warn($"A page with id {pageId} is not found.");
                    return;
                }

                var jobId = Guid.NewGuid();
                
                var json = JsonConvert.SerializeObject(new PdfPageGeneratorQueueMessage
                {
                    PageId = pageId,
                    PdfJobId = jobId,
                    ThumbnailJobId = jobId,
                    AgentUserId = agentUserId,
                    PageWidth = templateWidth,
                    PageHeight = templateHeight,
                    ThumbnailWidth = thumbnailWidth,
                    ThumbnailHeight = thumbnailHeight,
                    Dpi = dpi
                });
                _ordersQueueClient.Send(new BrokeredMessage(json));

                _notificationsBroadcaster.BroadcastPagePdfNotReady(page.PrintId);

                SetJobStatusInDatabase(page, jobId);
            }
            catch (Exception e)
            {
                Logger.Warn(e, $"The job for page with id {pageId} couldn't be added to the queue.");
            }
        }

        private void SetJobStatusInDatabase(Page page, Guid jobId)
        {
            var utcNow = DateTime.UtcNow;

            page.ThumbnailJobId = jobId;
            page.ThumbnailJobEnqueueTimeUtc = utcNow;
            page.ThumbnailJobStatus = JobStatus.InProgress;

            page.PdfJobId = jobId;
            page.PdfJobEnqueueTimeUtc = utcNow;
            page.PdfJobStatus = JobStatus.InProgress;

            _unitOfWork.PageRepository.Update(page);
            _unitOfWork.Save();
        }
    }
}