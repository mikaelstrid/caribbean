using System.Linq;
using System.Web.Mvc;
using Caribbean.Aruba.Web.ViewModels.RealEstateObject;
using Caribbean.DataAccessLayer.RealEstateObjects;

namespace Caribbean.Aruba.Web.Controllers
{
    [RoutePrefix("objekt")]
    public class RealEstateObjectController : Controller
    {
        private readonly IVitecObjectRepository _vitecObjectRepository;

        public RealEstateObjectController(IVitecObjectRepository vitecObjectRepository)
        {
            _vitecObjectRepository = vitecObjectRepository;
        }

        [Route("valj/{printTemplateName?}")]
        public ActionResult Choose(string printTemplateName)
        {
            //var agent = await _unitOfWork.AgentRepository.GetByUserId(User.Identity.GetUserId());
            //if (agent == null) return HttpNotFound("No agent associated with the username found.");

            //var availableObjects = _unitOfWork.VitecObjectRepository.GetAllSummariesForAgency(agent.Agency.VitecCustomerId);
            var availableObjects = _vitecObjectRepository.GetAllSummariesForAgency("26301");

            return View(new ChooseObjectViewModel
            {
                //PrintVariantSlug = t,
                AvailableObjects = availableObjects.Select(o => new ObjectSummaryViewModel
                {
                    Id = o.Id,
                    Address = o.Address,
                    ThumbailUrl = o.ThumbnailUrl
                })
            });
        }
    }
}
