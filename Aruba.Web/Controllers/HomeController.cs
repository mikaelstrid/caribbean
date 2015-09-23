using System.Web.Mvc;
using Caribbean.Aruba.Web.ViewModels;

namespace Caribbean.Aruba.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View(new FullTopbarLayoutViewModel { ActiveMenuItem = MenuItem.Start });
        }
    }
}