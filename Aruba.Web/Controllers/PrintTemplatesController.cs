using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Caribbean.Aruba.Web.ViewModels.PrintTemplates;
using Caribbean.DataAccessLayer.Database;
using Caribbean.DataAccessLayer.PrintTemplates;
using Microsoft.AspNet.Identity;

namespace Caribbean.Aruba.Web.Controllers
{
    [Authorize]
    [RoutePrefix("trycksaksmallar")]
    public class PrintTemplatesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITemplateMetadataRepository _templateMetadataRepository;

        public PrintTemplatesController(IUnitOfWork unitOfWork, ITemplateMetadataRepository templateMetadataRepository)
        {
            _unitOfWork = unitOfWork;
            _templateMetadataRepository = templateMetadataRepository;
        }

        [Route("valj/{o?}")]
        public async Task<ActionResult> Choose(string o)
        {
            var selectedObjectId = o;

            var agent = await _unitOfWork.AgentRepository.GetByUserId(User.Identity.GetUserId());
            if (agent == null) return HttpNotFound("No agent associated with the username found.");

            var metadataItems = _templateMetadataRepository.GetAllPrintTemplatesForAgency(agent.Agency.Slug);

            return View(new ChoosePrintTemplateModel
            {
                SelectedObjectId = selectedObjectId,
                AvailablePrintTypes = metadataItems.Where(t => t.IsValid).Select(t => new AvailablePrintTemplateViewModel
                {
                    Slug = t.Slug,
                    Type = t.Type,
                    SubType = t.Name,
                    Description = t.Description,
                    ThumbnailUrl = t.ThumbnailUrl
                })
            });
        }
    }
}