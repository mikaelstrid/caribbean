using System.Web.Configuration;
using Caribbean.Aruba.SharedTypes;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace Caribbean.Aruba.Web.Business
{
    public interface IPagePdfGeneratorProxyService
    {
        void Initialize();
        void QueueJob(int pageId, string agentUserId, int templateWidth, int templateHeight, int dpi, int thumbnailWidth, int thumbnailHeight);
    }

    public class PagePdfGeneratorProxyService : IPagePdfGeneratorProxyService
    {
        private QueueClient _ordersQueueClient;

        public void Initialize()
        {
            if (_ordersQueueClient != null) return;
            _ordersQueueClient = QueueClient.CreateFromConnectionString(
                WebConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"],
                WebConfigurationManager.AppSettings["Aruba.PdfWorkerQueueName"]);
        }

        public void QueueJob(int pageId, string agentUserId, int templateWidth, int templateHeight, int dpi, int thumbnailWidth, int thumbnailHeight)
        {
            var json = JsonConvert.SerializeObject(new PdfPageGeneratorQueueMessage
            {
                PageId = pageId,
                AgentUserId = agentUserId,
                PageWidth = templateWidth, 
                PageHeight = templateHeight,
                ThumbnailWidth = thumbnailWidth,
                ThumbnailHeight = thumbnailHeight,
                Dpi = dpi
            });
            _ordersQueueClient.Send(new BrokeredMessage(json));
        }
    }
}