using System.Web.Mvc;
using Caribbean.Aruba.Web.ViewModels;
using NLog;

namespace Caribbean.Aruba.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ActionResult Index()
        {
            return View(new FullTopbarLayoutViewModel { ActiveMenuItem = MenuItem.Start });
        }
    }
}