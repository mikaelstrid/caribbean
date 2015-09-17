using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Caribbean.DataAccessLayer.Database;
using Microsoft.AspNet.Identity;

namespace Caribbean.Aruba.Web.ApiControllers
{
    public class PrintsController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public PrintsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //// GET: api/Prints
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET: api/Prints/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // GET: api/Prints/5
        [Route("api/prints/{id}/pages")]
        public async Task<IHttpActionResult> GetPages(int id)
        {
            var agent = await _unitOfWork.AgentRepository.GetByUserId(User.Identity.GetUserId());
            if (agent == null) return NotFound();

            var print = await _unitOfWork.PrintRepository.GetSingle(p => p.Id == id, "Pages");
            if (print == null) return NotFound();

            return Ok(print.Pages.Select(p => new PageApiModel { Id = p.Id, Position = p.Position, PreviewUrl = p.ThumbnailUrl}));
        }

        public class PageApiModel
        {
            public int Id { get; set; }
            public int Position { get; set; }
            public string PreviewUrl { get; set; }
        }

        //// POST: api/Prints
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT: api/Prints/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE: api/Prints/5
        //public void Delete(int id)
        //{
        //}
    }
}
