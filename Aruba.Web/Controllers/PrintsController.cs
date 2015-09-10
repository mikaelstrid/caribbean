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

        public PrintsController(
            IUnitOfWork unitOfWork, 
            ITemplateMetadataRepository templateMetadataRepository, 
            ITemplateContentRepository templateContentRepository,
            IMuseTemplateParser museTemplateParser,
            IVitecObjectRepository vitecObjectRepository,
            IInitialFieldValuesFactory initialFieldValuesFactory)
        {
            _unitOfWork = unitOfWork;
            _templateMetadataRepository = templateMetadataRepository;
            _templateContentRepository = templateContentRepository;
            _museTemplateParser = museTemplateParser;
            _vitecObjectRepository = vitecObjectRepository;
            _initialFieldValuesFactory = initialFieldValuesFactory;
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


        [Route("redigera/{id}")]
        public async Task<ActionResult> Edit(int id)
        {
            var agent = await _unitOfWork.AgentRepository.GetByUserId(User.Identity.GetUserId());
            if (agent == null) return HttpNotFound("No agent associated with the username found.");

            var print = await _unitOfWork.PrintRepository.GetSingle(p => p.Id == id, "Pages");
            if (print == null) return HttpNotFound("No print with a matching id found.");

            var printVariantType = _templateMetadataRepository.GetPrintVariantBySlug(agent.Agency.Slug, print.PrintVariantSlug);
            if (printVariantType == null) return HttpNotFound("No print variant with a matching slug found.");

            return View(new DesignPrintViewModel
            {
                PrintId = print.Id,
                PrintVariantType = printVariantType.Type,
                Pages = print.Pages.Select(p => CreatePageViewModel(agent.Agency.Slug, p)).OrderBy(p => p.Position)
            });
        }

        private DesignPrintViewModel.PageViewModel CreatePageViewModel(string agencySlug, Page page)
        {
            var template = _templateMetadataRepository.GetPageTemplateBySlug(agencySlug, page.PageTemplateSlug);
            return new DesignPrintViewModel.PageViewModel
            {
                Id = page.Id,
                Position = page.Position,
                ThumbnailUrl = page.ThumbnailUrl ?? template.ThumbnailUrl,
                TemplateName = template.Name,
            };
        }
    }
}