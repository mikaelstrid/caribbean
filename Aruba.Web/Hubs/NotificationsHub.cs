using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Caribbean.Aruba.Web.Hubs
{
    public class NotificationsHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }
    }
}