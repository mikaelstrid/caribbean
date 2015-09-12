using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Caribbean.DataAccessLayer.Database;
using Caribbean.Models.Database;

namespace Caribbean.Aruba.Web.ApiControllers
{
    [Authorize]
    public class FieldValuesController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public FieldValuesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public string Get()
        {
            return "Hello World";
        }

        public async Task<IHttpActionResult> Post(CreateFieldValueApiModel model)
        {
            var existingFieldValue = await _unitOfWork.FieldValueRepository.GetSingle(v => v.PageId == model.PageId && v.FieldName == model.FieldName);
            if (existingFieldValue != null)
            {
                existingFieldValue.Value = model.FieldValue;
                _unitOfWork.FieldValueRepository.Update(existingFieldValue);
                _unitOfWork.Save();
                return Ok(existingFieldValue.Id);
            }
            else
            {
                var newFieldValue = new FieldValue
                {
                    PageId = model.PageId,
                    FieldName = model.FieldName,
                    Value = model.FieldValue
                };
                _unitOfWork.FieldValueRepository.Add(newFieldValue);
                _unitOfWork.Save();
                return Ok(newFieldValue.Id);
            }
        }

        public async Task<IHttpActionResult> Put(UpdateFieldValueApiModel model)
        {
            var existingFieldValue = await _unitOfWork.FieldValueRepository.GetById(model.FieldValueId);
            if (existingFieldValue == null) return NotFound();

            existingFieldValue.Value = model.FieldValue;
            _unitOfWork.FieldValueRepository.Update(existingFieldValue);
            _unitOfWork.Save();
            return Content(HttpStatusCode.Accepted, model);
        }
    }

    public class CreateFieldValueApiModel
    {
        public int PageId { get; set; }
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
    }

    public class UpdateFieldValueApiModel
    {
        public int FieldValueId { get; set; }
        public string FieldValue { get; set; }
    }


}
