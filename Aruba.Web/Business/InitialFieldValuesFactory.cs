using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Caribbean.DataAccessLayer.Database;
using Caribbean.DataAccessLayer.RealEstateObjects;
using Caribbean.Models.Database;
using Caribbean.Models.RealEstateObjects;
using Newtonsoft.Json;
using NLog;

namespace Caribbean.Aruba.Web.Business
{
    public interface IInitialFieldValuesFactory
    {
        Task<IEnumerable<FieldValue>> CreateInitialFieldValues(IEnumerable<FieldInfoBase> fields, VitecObjectDetails vitecObject);
    }

    public class InitialFieldValuesFactory : IInitialFieldValuesFactory
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IUnitOfWork _unitOfWork;
        private readonly IVitecObjectFactory _vitecObjectFactory;

        public InitialFieldValuesFactory(IUnitOfWork unitOfWork, IVitecObjectFactory vitecObjectFactory)
        {
            _unitOfWork = unitOfWork;
            _vitecObjectFactory = vitecObjectFactory;
        }

        public async Task<IEnumerable<FieldValue>> CreateInitialFieldValues(IEnumerable<FieldInfoBase> fields, VitecObjectDetails vitecObject)
        {
            var pageTemplatePlaceholderMappings = await _unitOfWork.PageTemplatePlaceholderMappingRepository.Get();
            var valueMappings = pageTemplatePlaceholderMappings.ToDictionary(m => m.Name, m => m.Path);

            var result = new List<FieldValue>();
            foreach (var field in fields)
            {
                if (field is TextFieldInfo)
                {
                    var fieldValue = CreateInitialTextFieldValue(field as TextFieldInfo, vitecObject, valueMappings);
                    if (fieldValue != null) result.Add(fieldValue);
                }
                else if (field is HtmlFieldInfo)
                {
                    var fieldValue = CreateInitialHtmlFieldValue(field as HtmlFieldInfo, vitecObject, valueMappings);
                    if (fieldValue != null) result.Add(fieldValue);
                }
                else if (field is ImageFieldInfo)
                {
                    var fieldValue = CreateInitialImageFieldValue(_vitecObjectFactory, field as ImageFieldInfo, vitecObject, valueMappings);
                    if (fieldValue != null) result.Add(fieldValue);
                }
            }
            return result;
        }

        private static FieldValue CreateInitialTextFieldValue(TextFieldInfo fieldInfo, VitecObjectDetails vitecObject, IReadOnlyDictionary<string, string> valueMappings)
        {
            var xpath = GetFieldPath(fieldInfo.FieldName, valueMappings);
            var vitecValue = vitecObject.GetElementValue(xpath);
            if (vitecValue == null) return null;
            return new FieldValue { FieldName = fieldInfo.FieldName, Value = JsonConvert.SerializeObject(new { html = vitecValue }) };
        }

        private static FieldValue CreateInitialHtmlFieldValue(HtmlFieldInfo fieldInfo, VitecObjectDetails vitecObject, IReadOnlyDictionary<string, string> valueMappings)
        {
            var xpath = GetFieldPath(fieldInfo.FieldName, valueMappings);
            var vitecValue = vitecObject.GetElementValue(xpath);
            if (vitecValue == null) return null;
            if (!vitecValue.StartsWith("<p")) vitecValue = $"<p class=\"{fieldInfo.FirstParagraphClass}\">{vitecValue}</p>";
            return new FieldValue { FieldName = fieldInfo.FieldName, Value = JsonConvert.SerializeObject(new { html = vitecValue }) };
        }
        
        internal static FieldValue CreateInitialImageFieldValue(IVitecObjectFactory vitecObjectFactory, ImageFieldInfo fieldInfo, VitecObjectDetails vitecObject, IReadOnlyDictionary<string, string> valueMappings)
        {
            var xpath = GetFieldPath(fieldInfo.FieldName, valueMappings);
            if (xpath == null)
            {
                Logger.Warn($"Field {fieldInfo.FieldName} does not exist.");
                return null;
            }

            var pictureElement = vitecObject.GetElement(xpath);
            if (pictureElement == null)
            {
                Logger.Warn($"Field {fieldInfo.FieldName} with XPath {xpath} has no matches.");
                return null;
            }

            var image =  vitecObjectFactory.CreateImage(pictureElement);
            if (image == null)
            {
                Logger.Warn($"Could not create image for element {pictureElement}.");
                return null;
            }

            return new FieldValue
            {
                FieldName = fieldInfo.FieldName,
                Value = JsonConvert.SerializeObject(new
                {
                    url = image.GetImageUrl(),
                    scale = 1.0,
                    angle = 0,
                    x = 0,
                    y = 0,
                    b = 1189,
                    h = 793
                })
            };
        }


        internal static string GetFieldPath(string fieldName, IReadOnlyDictionary<string, string> valueMappings)
        {
            if (string.IsNullOrWhiteSpace(fieldName) || valueMappings == null || !valueMappings.Any()) return null;
            return valueMappings.ContainsKey(fieldName) ? valueMappings[fieldName] : null;
        }
    }
}
