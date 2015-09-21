using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Caribbean.Aruba.Web.Business;
using Caribbean.Aruba.Web.ViewModels.Prints;
using Caribbean.DataAccessLayer.Database;
using Caribbean.DataAccessLayer.PrintTemplates;
using Caribbean.DataAccessLayer.RealEstateObjects;
using Caribbean.Models.Database;
using Microsoft.AspNet.Identity;

namespace Caribbean.Aruba.Web.Controllers
{
    [Authorize]
    [RoutePrefix("trycksaker")]
    public class PrintsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITemplateMetadataRepository _templateMetadataRepository;
        private readonly ITemplateContentRepository _templateContentRepository;
        private readonly IMuseTemplateParser _museTemplateParser;
        private readonly IVitecObjectRepository _vitecObjectRepository;
        private readonly IInitialFieldValuesFactory _initialFieldValuesFactory;
        private readonly IPrintPdfGeneratorService _printPdfGeneratorService;
        private readonly IPrintPdfRepository _printPdfRepository;
        private readonly IPagePdfGeneratorProxyService _pagePdfGeneratorProxyService;

        public PrintsController(
            IUnitOfWork unitOfWork, 
            ITemplateMetadataRepository templateMetadataRepository, 
            ITemplateContentRepository templateContentRepository,
            IMuseTemplateParser museTemplateParser,
            IVitecObjectRepository vitecObjectRepository,
            IInitialFieldValuesFactory initialFieldValuesFactory,
            IPrintPdfGeneratorService printPdfGeneratorService,
            IPrintPdfRepository printPdfRepository,
            IPagePdfGeneratorProxyService pagePdfGeneratorProxyService)
        {
            _unitOfWork = unitOfWork;
            _templateMetadataRepository = templateMetadataRepository;
            _templateContentRepository = templateContentRepository;
            _museTemplateParser = museTemplateParser;
            _vitecObjectRepository = vitecObjectRepository;
            _initialFieldValuesFactory = initialFieldValuesFactory;
            _printPdfGeneratorService = printPdfGeneratorService;
            _printPdfRepository = printPdfRepository;
            _pagePdfGeneratorProxyService = pagePdfGeneratorProxyService;
        }

        [Route]
        public async Task<ActionResult> Index()
        {
            var agent = await _unitOfWork.AgentRepository.GetByUserId(User.Identity.GetUserId());
            if (agent == null) return HttpNotFound("No agent associated with the username found.");

            var prints = await _unitOfWork.PrintRepository.Get(p => p.AgentId == agent.Id);

            return View(new IndexPrintsViewModel
            {
                Prints = prints.Select(p => CreatePrintViewModel(agent, p))
            });
        }

        private IndexPrintsViewModel.PrintViewModel CreatePrintViewModel(Agent agent, Print print)
        {
            var realEstateObject = _vitecObjectRepository.GetSummaryById(agent.Agency.VitecCustomerId, print.ObjectId);
            var template = _templateMetadataRepository.GetPrintVariantBySlug(agent.Agency.Slug, print.PrintVariantSlug);
            return new IndexPrintsViewModel.PrintViewModel
            {
                Address = realEstateObject.Address,
                ThumbnailUrl = realEstateObject.ThumbnailUrl,
                TemplateType = template.Type,
                TemplateName = template.Name,
                Status = print.Status,
                CreationTimeUtc = print.CreationTimeUtc,
                PdfUrl = print.PdfUrl
            };
        }




        [Route("skapa")]
        public async Task<ActionResult> Create(string t, string o)
        {
            var templateSlug = t;
            var objectId = o;

            var agent = await _unitOfWork.AgentRepository.GetByUserId(User.Identity.GetUserId());
            if (agent == null) return HttpNotFound("No agent associated with the username found.");

            var printVariantMetadata = _templateMetadataRepository.GetPrintVariantBySlug(agent.Agency.Slug, templateSlug);

            var print = new Print
            {
                AgentId = agent.Id,
                CreationTimeUtc = DateTime.UtcNow,
                ModifiedTimeUtc = DateTime.UtcNow,
                ObjectId = objectId,
                PrintVariantSlug = templateSlug,
                Pages = new List<Page>()
            };

            var pagePosition = 0;
            foreach (var proposedPageSlug in printVariantMetadata.ProposedPageSlugs)
            {
                pagePosition++;
                var page = new Page { Position = pagePosition, PageTemplateSlug = proposedPageSlug, FieldValues = new List<FieldValue>() };

                var templateHtml = _templateContentRepository.GetPageTemplateBySlug(agent.Agency.Slug, proposedPageSlug);
                var templateFields = _museTemplateParser.FindAllFields(templateHtml);
                var vitecObject = _vitecObjectRepository.GetDetailsById(objectId);
                var initialFieldValues = await _initialFieldValuesFactory.CreateInitialFieldValues(templateFields, vitecObject);

                foreach (var v in initialFieldValues)
                {
                    page.FieldValues.Add(v);
                }

                print.Pages.Add(page);
            }

            _unitOfWork.PrintRepository.Add(print);
            _unitOfWork.Save();

            return RedirectToAction("Edit", new { id = print.Id });
        }


        [Route("redigera")]
        public async Task<ActionResult> Edit(int id)
        {
            var agent = await _unitOfWork.AgentRepository.GetByUserId(User.Identity.GetUserId());
            if (agent == null) return HttpNotFound("No agent associated with the username found.");

            var print = await _unitOfWork.PrintRepository.GetSingle(p => p.Id == id, "Pages");
            if (print == null) return HttpNotFound("No print with a matching id found.");

            var printVariantType = _templateMetadataRepository.GetPrintVariantBySlug(agent.Agency.Slug, print.PrintVariantSlug);
            if (printVariantType == null) return HttpNotFound("No print variant with a matching slug found.");

            return View(new EditPrintViewModel
            {
                PrintId = print.Id,
                PrintVariantType = printVariantType.Type,
                Pages = print.Pages.Select(p => CreatePageViewModel(p, agent.Agency.Slug)).OrderBy(p => p.Position)
            });
        }

        private EditPrintViewModel.PageViewModel CreatePageViewModel(Page page, string agencySlug)
        {
            var pageTemplate = page == null ? null : _templateMetadataRepository.GetPageTemplateBySlug(agencySlug, page.PageTemplateSlug);
            if (page == null || pageTemplate == null) return null;
            return new EditPrintViewModel.PageViewModel
            {
                Id = page.Id,
                Position = page.Position,
                PreviewUrl = Url.Action("Editor", "Page", new { id = page.Id }),
                Width = pageTemplate.Width,
                Height = pageTemplate.Height,
            };
        }



        [HttpPost]
        [Route("redigera")]
        public async Task<ActionResult> Edit(EditPageSaveChangesViewModel viewModel)
        {
            _pagePdfGeneratorProxyService.Initialize();

            var agentUserId = User.Identity.GetUserId();
            var agent = await _unitOfWork.AgentRepository.GetByUserId(agentUserId);

            var print = await _unitOfWork.PrintRepository.GetSingle(p => p.Id == viewModel.PrintId, "Pages");
            if (print == null) return HttpNotFound("No print with a matching id found.");

            foreach (var page in print.Pages)
            {
                var template = _templateMetadataRepository.GetPageTemplateBySlug(agent.Agency.Slug, page.PageTemplateSlug);
                if (template == null) return HttpNotFound($"Page template {page.PageTemplateSlug} not found.");

                _pagePdfGeneratorProxyService.QueueJob(page.Id, agentUserId, template.Width, template.Height, template.Dpi, 220, 308);
            }
            return RedirectToAction("Index", "Home");
        }




        [Route("bestall/{id}")]
        public async Task<ActionResult> Order(int id)
        {
            var agent = await _unitOfWork.AgentRepository.GetByUserId(User.Identity.GetUserId());
            if (agent == null) return HttpNotFound("No agent associated with the username found.");

            var print = await _unitOfWork.PrintRepository.GetSingle(p => p.Id == id, "Pages");
            if (print == null) return HttpNotFound("No print with a matching id found.");

            var viewModel = new OrderPrintViewModel
            {
                AllPagePdfsGenerated = print.Pages.All(p => !string.IsNullOrWhiteSpace(p.PdfUrl)),
            };

            if (viewModel.AllPagePdfsGenerated)
            {
                var result = _printPdfGeneratorService.GeneratePdf(print);
                if (result != null)
                {
                    viewModel.PrintPdfUrl = result.PdfUrl;
                    UpdatePrintWithNewPdf(print, result.PdfName, result.PdfUrl);
                }
            }

            return View(viewModel);
        }

        private void UpdatePrintWithNewPdf(Print print, string printPdfName, string printPdfUrl)
        {
            if (!string.IsNullOrWhiteSpace(print.PdfName)) _printPdfRepository.Delete(print.PdfName);

            print.PdfName = printPdfName;
            print.PdfUrl = printPdfUrl;
            _unitOfWork.PrintRepository.Update(print);
            _unitOfWork.Save();
        }

    }
}