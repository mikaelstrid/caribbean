using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Caribbean.DataAccessLayer.Database;
using Caribbean.DataAccessLayer.PrintTemplates;
using Caribbean.Models.Database;
using Microsoft.AspNet.Identity;

namespace Caribbean.Aruba.Web.ApiControllers
{
    public class PageController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITemplateMetadataRepository _templateMetadataRepository;

        public PageController(IUnitOfWork unitOfWork, ITemplateMetadataRepository templateMetadataRepository)
        {
            _unitOfWork = unitOfWork;
            _templateMetadataRepository = templateMetadataRepository;
        }

        [Route("api/page/{id}")]
        public async Task<IHttpActionResult> Get(int id)
        {
            var agent = await _unitOfWork.AgentRepository.GetByUserId(User.Identity.GetUserId());
            if (agent == null) return Unauthorized();

            var requestedPage = (await _unitOfWork.PageRepository.Get(p => p.Id == id, includeProperties: "FieldValues")).FirstOrDefault();
            if (requestedPage == null) return NotFound();

            var model = CreatePageViewModel(_templateMetadataRepository, requestedPage, agent.Agency.Slug);

            if (model != null)
                return Ok(model);
            else 
                return NotFound();
        }

        private PageViewModel CreatePageViewModel(ITemplateMetadataRepository templateMetadataRepository, Page page, string agencySlug)
        {
            var pageTemplate = templateMetadataRepository.GetPageTemplateBySlug(agencySlug, page.PageTemplateSlug);
            if (pageTemplate == null) return null;
            
            return new PageViewModel
            {
                Id = page.Id,
                Position = page.Position,
                PreviewUrl = Url.Link("PageEditorRoute", new { id = page.Id }),
                Width = pageTemplate.Width,
                Height = pageTemplate.Height,
                FieldValues = page.FieldValues.Select(fv => new FieldValueViewModel { Id = fv.Id, Name = fv.FieldName, Value = fv.Value})
            };
        }

        public class PageViewModel
        {
            public int Id { get; set; }
            public int Position { get; set; }
            public string PreviewUrl { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public double AspectRatioInPercent => ((double)Height / (double)Width) * 100;

            public IEnumerable<FieldValueViewModel> FieldValues { get; set; }
        }

        public class FieldValueViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
        }


        //// POST: api/Page
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT: api/Page/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE: api/Page/5
        //public void Delete(int id)
        //{
        //}
    }
}
