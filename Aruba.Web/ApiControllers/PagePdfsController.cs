using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Caribbean.Aruba.SharedTypes;
using Caribbean.Aruba.Web.Hubs;
using Caribbean.DataAccessLayer.Database;
using Caribbean.DataAccessLayer.PrintTemplates;
using Caribbean.Models.Database;

namespace Caribbean.Aruba.Web.ApiControllers
{
    [Authorize]
    public class PagePdfsController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPagePdfRepository _pagePdfRepository;
        private readonly INotificationsBroadcaster _notificationsBroadcaster;

        public PagePdfsController(IUnitOfWork unitOfWork, IPagePdfRepository pagePdfRepository, INotificationsBroadcaster notificationsBroadcaster)
        {
            _unitOfWork = unitOfWork;
            _pagePdfRepository = pagePdfRepository;
            _notificationsBroadcaster = notificationsBroadcaster;
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> Post(PostAssetApiModel model)
        {
            var existingPage = await _unitOfWork.PageRepository.GetById(model.PageId);
            if (existingPage == null) return NotFound();

            var agent = await _unitOfWork.AgentRepository.GetByUserId(model.AgentUserId);
            if (agent == null) return Unauthorized();
            
            if (!string.IsNullOrWhiteSpace(existingPage.PdfName)) _pagePdfRepository.Delete(existingPage.PdfName);
            
            if (existingPage.PdfJobId == model.JobId) // Otherwise another Id is queued later
            {
                existingPage.PdfJobStatus = JobStatus.Completed;
                existingPage.PdfJobCompletionTimeUtc = DateTime.UtcNow;
            }
            existingPage.PdfJobDurationMs = model.JobDurationMs;
            existingPage.PdfName = model.AssetName;
            existingPage.PdfUrl = model.AssetUrl;
            _unitOfWork.PageRepository.Update(existingPage);
            _unitOfWork.Save();

            var print = (await _unitOfWork.PrintRepository.Get(p => p.Id == existingPage.PrintId, includeProperties: "Pages")).FirstOrDefault();
            if (print != null && print.Pages.All(p => p.PdfJobStatus == JobStatus.Completed))
            {
                _notificationsBroadcaster.BroadcastAllPagePdfsReady(print.Id);
            }

            return Ok();
        }
    }
}
