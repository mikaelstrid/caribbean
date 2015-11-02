using Microsoft.AspNet.SignalR;

namespace Caribbean.Aruba.Web.Hubs
{
    public interface INotificationsBroadcaster
    {
        void SubscribeToPrintChanges(string connectionId, int printId);
        void BroadcastPageThumbnailUpdate(int printId, int pageId, string thumbnailUrl);
        void BroadcastAllPagePdfsReady(int printId);
        void BroadcastPagePdfNotReady(int printId);
    }

    public class NotificationsBroadcaster : INotificationsBroadcaster
    {
        private readonly IHubContext _hubContext;

        public NotificationsBroadcaster()
        {
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationsHub>();
        }

        public void SubscribeToPrintChanges(string connectionId, int printId)
        {
            _hubContext.Groups.Add(connectionId, CreatePrintSubscribesGroupName(printId));
        }

        public void BroadcastPageThumbnailUpdate(int printId, int pageId, string thumbnailUrl)
        {
            _hubContext.Clients
                .Group(CreatePrintSubscribesGroupName(printId))
                .pageThumbnailUpdated(pageId, thumbnailUrl);
        }

        public void BroadcastAllPagePdfsReady(int printId)
        {
            _hubContext.Clients
                .Group(CreatePrintSubscribesGroupName(printId))
                .allPagePdfsReady(printId);
        }

        public void BroadcastPagePdfNotReady(int printId)
        {
            _hubContext.Clients
                .Group(CreatePrintSubscribesGroupName(printId))
                .pagePdfNotReady(printId);
        }


        private static string CreatePrintSubscribesGroupName(int printId)
        {
            return $"Print{printId}_Subscribers";
        }
    }
}
