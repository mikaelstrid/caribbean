using Microsoft.AspNet.SignalR;

namespace Caribbean.Aruba.Web.Hubs
{
    public class NotificationsHub : Hub
    {
        private readonly INotificationsBroadcaster _notificationsBroadcaster;

        public NotificationsHub(INotificationsBroadcaster notificationsBroadcaster)
        {
            _notificationsBroadcaster = notificationsBroadcaster;
        }

        public void SubscribeToPrintChanges(int printId)
        {
            _notificationsBroadcaster.SubscribeToPrintChanges(Context.ConnectionId, printId);
        }
    }
}