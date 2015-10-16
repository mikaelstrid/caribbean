using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Caribbean.Aruba.Web.ViewModels.RealEstateObject;
using Caribbean.DataAccessLayer.Database;
using Caribbean.DataAccessLayer.RealEstateObjects;
using Microsoft.AspNet.Identity;

namespace Caribbean.Aruba.Web.Controllers
{
    [Authorize]
    [RoutePrefix("objekt")]
    public class RealEstateObjectsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVitecObjectRepository _vitecObjectRepository;

        public RealEstateObjectsController(IUnitOfWork unitOfWork, IVitecObjectRepository vitecObjectRepository)
        {
            _unitOfWork = unitOfWork;
            _vitecObjectRepository = vitecObjectRepository;
        }

        [Route("valj/{t?}")]
        public async Task<ActionResult> Choose(string t)
        {
            var selectedPrintTemplateName = t;

            var agent = await _unitOfWork.AgentRepository.GetByUserId(User.Identity.GetUserId());
            if (agent == null) return HttpNotFound("No agent associated with the username found.");

            var availableObjects = _vitecObjectRepository.GetAllSummariesForAgency(agent.Agency.VitecCustomerId);

            return View(new ChooseObjectViewModel
            {
                SelectedPrintTemplateSlug = selectedPrintTemplateName,
                AvailableObjects = availableObjects.OrderBy(o => o.Status).ThenByDescending(o => o.CreatedTime).Select(o => new ObjectSummaryViewModel
                {
                    Id = o.Id,
                    Address = o.Address,
                    CityArea = o.CityArea,
                    SquareMeters = o.SquareMeters,
                    NumberOfRooms = o.NumberOfRooms,
                    ThumbailUrl = o.ThumbnailUrl,
                    Price = o.Price,
                    CreatedTime = o.CreatedTime,
                    Status = o.Status
                })
            });
        }

        public ActionResult EmptyCache(string t)
        {
            _vitecObjectRepository.EmptyCache();
            return RedirectToAction("Choose", new { t });
        }
    }
}
