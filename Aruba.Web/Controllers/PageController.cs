using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Caribbean.Aruba.Web.Business;
using Caribbean.Aruba.Web.ViewModels.Page;
using Caribbean.DataAccessLayer.Database;
using Caribbean.DataAccessLayer.PrintTemplates;
using Caribbean.DataAccessLayer.RealEstateObjects;
using Caribbean.Models.Database;
using Microsoft.AspNet.Identity;

namespace Caribbean.Aruba.Web.Controllers
{
    [Authorize]
    [RoutePrefix("sida")]
    public class PageController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITemplateMetadataRepository _templateMetadataRepository;
        private readonly ITemplateContentRepository _templateContentRepository;
        private readonly IPageFactory _pageFactory;
        private readonly IVitecObjectRepository _vitecObjectRepository;
        private readonly IPagePdfGeneratorProxyService _pagePdfGeneratorProxyService;

        public PageController(IUnitOfWork unitOfWork, ITemplateMetadataRepository templateMetadataRepository, ITemplateContentRepository templateContentRepository, IPageFactory pageFactory, IVitecObjectRepository vitecObjectRepository, IPagePdfGeneratorProxyService pagePdfGeneratorProxyService)
        {
            _unitOfWork = unitOfWork;
            _templateMetadataRepository = templateMetadataRepository;
            _templateContentRepository = templateContentRepository;
            _pageFactory = pageFactory;
            _vitecObjectRepository = vitecObjectRepository;
            _pagePdfGeneratorProxyService = pagePdfGeneratorProxyService;
        }


        [Route("redigera")]
        public async Task<ActionResult> Edit(int id)
        {
            var agent = await _unitOfWork.AgentRepository.GetByUserId(User.Identity.GetUserId());
            if (agent == null) return HttpNotFound("No agent associated with the username found.");

            var requestedPage = await _unitOfWork.PageRepository.GetSingle(p => p.Id == id, "Print");
            if (requestedPage == null) return HttpNotFound("No page associated with the id found.");

            return View(new EditPageViewModel
            {
                PrintId = requestedPage.PrintId,
                Page = CreatePageViewModel(_unitOfWork, requestedPage, agent.Agency.Slug),
                ObjectImages = GetObjectImages(requestedPage.Print.ObjectId, _unitOfWork)
            });
        }

        private EditPageViewModel.PageViewModel CreatePageViewModel(IUnitOfWork unitOfWork, Page page, string agencySlug)
        {
            var pageTemplate = page == null ? null : _templateMetadataRepository.GetPageTemplateBySlug(agencySlug, page.PageTemplateSlug);
            if (page == null || pageTemplate == null) return null;
            return new EditPageViewModel.PageViewModel
            {
                Id = page.Id,
                Position = page.Position,
                PreviewUrl = Url.Action("Editor", "Page", new { id = page.Id }),
                Width = pageTemplate.Width,
                Height = pageTemplate.Height,
            };
        }

        private IEnumerable<EditPageViewModel.ObjectImageViewModel> GetObjectImages(string objectId, IUnitOfWork unitOfWork)
        {
            var vitecObject = _vitecObjectRepository.GetDetailsById(objectId);
            return vitecObject.Images.Select(i => new EditPageViewModel.ObjectImageViewModel
            {
                PictureUrl = i.GetImageUrl(),
                ThumbnailUrl = i.GetThumbnailUrl(width: 250)
            });
        }




        [HttpPost]
        [Route("redigera")]
        public async Task<ActionResult> Edit(EditPageSaveChangesViewModel viewModel)
        {
            _pagePdfGeneratorProxyService.Initialize();

            var agentUserId = User.Identity.GetUserId();
            var agent = await _unitOfWork.AgentRepository.GetByUserId(agentUserId);

            if (viewModel.PageId != -1)
            {
                var page = await _unitOfWork.PageRepository.GetById(viewModel.PageId);
                if (page == null) return HttpNotFound($"No page associated with id {viewModel.PageId} found.");

                var template = _templateMetadataRepository.GetPageTemplateBySlug(agent.Agency.Slug, page.PageTemplateSlug);
                if (template == null) return HttpNotFound($"Page template {page.PageTemplateSlug} not found.");

                _pagePdfGeneratorProxyService.QueueJob(viewModel.PageId, agentUserId, template.Width, template.Height, template.Dpi, 220, 308);
            }

            return RedirectToAction("Edit", "Prints", new { id = viewModel.PrintId });
        }




        [Route("editor/{id}")]
        public async Task<ActionResult> Editor(int id)
        {
            var agent = await _unitOfWork.AgentRepository.GetByUserId(User.Identity.GetUserId());
            if (agent == null) return HttpNotFound("No agent associated with the username found.");

            var requestedPage = await _unitOfWork.PageRepository.GetById(id);
            if (requestedPage == null) return HttpNotFound("No page associated with the id found.");

            var templateContent = _templateContentRepository.GetPageTemplateBySlug(agent.Agency.Slug, requestedPage.PageTemplateSlug);
            if (templateContent == null) return HttpNotFound("No page template associated with the id found.");

            var editorHtml = _pageFactory.CreatePageEditorHtmlString(templateContent, requestedPage);

            return Content(editorHtml);
        }


        [AllowAnonymous]
        [Route("rendera")]
        public async Task<ActionResult> Render(string agentUserId, int pageId)
        {
            var agent = await _unitOfWork.AgentRepository.GetByUserId(agentUserId);
            if (agent == null) return HttpNotFound("No agent associated with the username found.");

            var requestedPage = await _unitOfWork.PageRepository.GetById(pageId);
            if (requestedPage == null) return HttpNotFound("No page associated with the id found.");

            var templateContent = _templateContentRepository.GetPageTemplateBySlug(agent.Agency.Slug, requestedPage.PageTemplateSlug);
            if (templateContent == null) return HttpNotFound("No page template associated with the id found.");

            var editorHtml = _pageFactory.CreatePageRenderHtmlString(templateContent, requestedPage);

            return Content(editorHtml);
        }
    }
}