using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Caribbean.DataAccessLayer.Database;
using Caribbean.DataAccessLayer.RealEstateObjects;
using Caribbean.Models.RealEstateObjects;
using Microsoft.AspNet.Identity;

namespace Caribbean.Aruba.Web.ApiControllers
{
    public class RealEstateObjectsController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVitecObjectRepository _vitecObjectRepository;

        public RealEstateObjectsController(IUnitOfWork unitOfWork, IVitecObjectRepository vitecObjectRepository)
        {
            _unitOfWork = unitOfWork;
            _vitecObjectRepository = vitecObjectRepository;
        }


        [Route("api/realestateobjects/{id}/images")]
        public async Task<IHttpActionResult> GetImages(string id)
        {
            var agent = await _unitOfWork.AgentRepository.GetByUserId(User.Identity.GetUserId());
            if (agent == null) return Unauthorized();

            var realEstateObject = _vitecObjectRepository.GetDetailsById(objectId: id);

            return Ok(new ImagesApiModel {
                ObjectImages = realEstateObject.ObjectImages.Select(i => new ImageApiModel
                {
                    ImageUrl = i.GetImageUrl(),
                    ThumbnailUrl = i.GetThumbnailUrl(width: 250)
                }),
                StaffImages = realEstateObject.StaffImages.Select(i => new ImageApiModel
                {
                    ImageUrl = i.GetImageUrl(),
                    ThumbnailUrl = i.GetThumbnailUrl(width: 250)
                })
            });
        }
        
        public class ImagesApiModel
        {
            public IEnumerable<ImageApiModel> ObjectImages { get; set; }
            public IEnumerable<ImageApiModel> StaffImages { get; set; }
        }

        public class ImageApiModel
        {
            public string ImageUrl { get; set; }
            public string ThumbnailUrl { get; set; }
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
