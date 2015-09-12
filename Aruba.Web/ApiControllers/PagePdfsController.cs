using System.Threading.Tasks;
using System.Web.Http;
using Caribbean.Aruba.SharedTypes;
using Caribbean.DataAccessLayer.Database;
using Caribbean.DataAccessLayer.PrintTemplates;

namespace Caribbean.Aruba.Web.ApiControllers
{
    [Authorize]
    public class PagePdfsController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPagePdfRepository _pagePdfRepository;

        public PagePdfsController(IUnitOfWork unitOfWork, IPagePdfRepository pagePdfRepository)
        {
            _unitOfWork = unitOfWork;
            _pagePdfRepository = pagePdfRepository;
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> Post(PostAssetApiModel model)
        {
            var existingPage = await _unitOfWork.PageRepository.GetById(model.PageId);
            if (existingPage == null) return NotFound();

            var agent = await _unitOfWork.AgentRepository.GetByUserId(model.AgentUserId);
            if (agent == null) return Unauthorized();
            
            if (!string.IsNullOrWhiteSpace(existingPage.PdfName)) _pagePdfRepository.Delete(existingPage.PdfName);

            existingPage.PdfName = model.AssetName;
            existingPage.PdfUrl = model.AssetUrl;
            _unitOfWork.PageRepository.Update(existingPage);
            _unitOfWork.Save();
            return Ok();
        }
    }
}
