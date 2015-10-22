using System;
using System.Threading.Tasks;
using System.Web.Http;
using Caribbean.Aruba.SharedTypes;
using Caribbean.DataAccessLayer.Database;
using Caribbean.DataAccessLayer.PrintTemplates;
using Caribbean.Models.Database;

namespace Caribbean.Aruba.Web.ApiControllers
{
    [Authorize]
    public class PageThumbnailsController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPageThumbnailRepository _pageThumbnailRepository;

        public PageThumbnailsController(IUnitOfWork unitOfWork, IPageThumbnailRepository pageThumbnailRepository)
        {
            _unitOfWork = unitOfWork;
            _pageThumbnailRepository = pageThumbnailRepository;
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> Post(PostAssetApiModel model)
        {
            var existingPage = await _unitOfWork.PageRepository.GetById(model.PageId);
            if (existingPage == null) return NotFound();

            var agent = await _unitOfWork.AgentRepository.GetByUserId(model.AgentUserId);
            if (agent == null) return Unauthorized();
            
            if (!string.IsNullOrWhiteSpace(existingPage.ThumbnailName)) _pageThumbnailRepository.Delete(existingPage.ThumbnailName);

            if (existingPage.ThumbnailJobId == model.JobId) // Otherwise another Id is queued later
            {
                existingPage.ThumbnailJobStatus = JobStatus.Completed; 
                existingPage.ThumbnailJobCompletionTimeUtc = DateTime.UtcNow;
            }
            existingPage.ThumbnailJobDurationMs = model.JobDurationMs;
            existingPage.ThumbnailName = model.AssetName;
            existingPage.ThumbnailUrl = model.AssetUrl;
            _unitOfWork.PageRepository.Update(existingPage);
            _unitOfWork.Save();
            return Ok();
        }
    }
}
