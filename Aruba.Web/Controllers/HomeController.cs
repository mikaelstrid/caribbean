using System.Diagnostics;
using System.Web.Mvc;
using Caribbean.Aruba.Web.ViewModels;

namespace Caribbean.Aruba.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Trace.TraceError("Hå och hej nu gick det fel!");

            return View(new FullTopbarLayoutViewModel { ActiveMenuItem = MenuItem.Start });
        }
    }
}