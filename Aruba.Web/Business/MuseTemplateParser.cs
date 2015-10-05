using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Caribbean.Models.Database;
using Newtonsoft.Json.Linq;
// ReSharper disable InconsistentNaming

namespace Caribbean.Aruba.Web.Business
{
    public interface IMuseTemplateParser
    {
        IEnumerable<FieldInfoBase> FindAllFields(string templateHtml);
        string MarkAllFields(string templateHtml, ICollection<FieldValue> fieldValues);
    }

    public class MuseTemplateParser : IMuseTemplateParser
    {
        private static readonly Regex REGEX_TEXT_FIELDS = new Regex("\\^:([^\\|]*?):\\^");
        //private static readonly Regex REGEX_HTML_FIELDS = new Regex("(<p[^\\>]*?class=\\\"([^\\\"]*?)\\\"[^<]*?)\\^:([^:]*)(\\|)(.*?):\\^(<\\/p>)", RegexOptions.Singleline);
        //private static readonly Regex REGEX_HTML_FIELDS = new Regex("(<p.*?)\\^:([^:]*)(\\|)(.*?):\\^(<\\/p>)", RegexOptions.Singleline);
        //private static readonly Regex REGEX_HTML_FIELDS = new Regex("(<p.*?)\\^:(.*)\\|(.*?):\\^(<\\/p>)", RegexOptions.Singleline);
        private static readonly Regex REGEX_HTML_FIELDS = new Regex("(<p[^\\>]*?[^<]*?)\\^:([^:]*)(\\|)(.*?):\\^(<\\/p>)", RegexOptions.Singleline);


        //:TODO:
        //private static readonly string REGEX_IMAGE_FIELDS_TYPE1_TEMPLATE = "(<div id=\\\".*?_clip\\\")>\\s*<img.*?(id=.*?)(src=\\\".*?)(\\/{0}-)(.*?)(\\..*?\\\")(.*?)( width=\\\".*?\\\" height=\\\".*?\\\").*?\\/>";

        private static readonly Regex REGEX_IMAGE_FIELDS_TYPE1 = new Regex("(<div id=\\\".*?_clip\\\")>\\s*<img.*?(id=.*?)(src=\\\".*?)(\\/bild-)(.*?)(\\..*?\\\")(.*?)( width=\\\".*?\\\" height=\\\".*?\\\").*?\\/>");
        private static readonly Regex REGEX_IMAGE_FIELDS_TYPE2 = new Regex("(<div class=\\\"clip_)([^\\>]*?\\\")(>\\s*<!-- image -->\\s*<img.*?)(id=.*?)(src=\\\".*?)(\\/bild-)(.*?)(\\..*?\")(.*?)( width=\\\".*?\\\" height=\\\".*?\\\")(.*?\\/>)");


        // === FIND FIELDS METHODS ===

        public IEnumerable<FieldInfoBase> FindAllFields(string templateHtml)
        {
            var textFields = FindAllTextFields(templateHtml);
            var htmlFields = FindAllHtmlFields(templateHtml);
            var imageType1Fields = FindAllImageType1Fields(templateHtml);
            var imageType2Fields = FindAllImageType2Fields(templateHtml);

            var result = new List<FieldInfoBase>();
            result.AddRange(textFields);
            result.AddRange(htmlFields);
            result.AddRange(imageType1Fields);
            result.AddRange(imageType2Fields);
            return result;
        } 

        internal static List<TextFieldInfo> FindAllTextFields(string templateHtml)
        {
            return REGEX_TEXT_FIELDS.Matches(templateHtml).Cast<Match>().Select(m => new TextFieldInfo { FieldName = m.Groups[1].Value }).ToList();
        }

        internal static List<HtmlFieldInfo> FindAllHtmlFields(string templateHtml)
        {
            return REGEX_HTML_FIELDS.Matches(templateHtml).Cast<Match>().Select(m => new HtmlFieldInfo { FieldName = m.Groups[2].Value, FirstParagraphClass = GetFirstParagraphClass(m.Groups[1].Value) }).ToList();
        }

        private static string GetFirstParagraphClass(string firstParagraghTag)
        {
            var regex = new Regex(".*?class=\\\"(.*?)\\\".*");
            var matches = regex.Matches(firstParagraghTag);
            return matches.Count > 0 ? matches[0].Groups[1].Value : "";
        }


        internal static List<ImageFieldInfo> FindAllImageType1Fields(string templateHtml)
        {
            return REGEX_IMAGE_FIELDS_TYPE1.Matches(templateHtml).Cast<Match>().Select(m => new ImageFieldInfo { FieldName = m.Groups[5].Value }).ToList();
        }

        internal static List<ImageFieldInfo> FindAllImageType2Fields(string templateHtml)
        {
            return REGEX_IMAGE_FIELDS_TYPE2.Matches(templateHtml).Cast<Match>().Select(m => new ImageFieldInfo { FieldName = m.Groups[7].Value }).ToList();
        }



        // === MARK/REPLACE FIELDS METHODS ===
        
        public string MarkAllFields(string templateHtml, ICollection<FieldValue> fieldValues)
        {
            var updatedHtml = templateHtml;
            updatedHtml = MarkEditableTextFields(updatedHtml, fieldValues);
            updatedHtml = MarkEditableHtmlFields(updatedHtml, fieldValues);
            updatedHtml = MarkEditableImageType1Fields(updatedHtml, fieldValues);
            updatedHtml = MarkEditableImageType2Fields(updatedHtml, fieldValues);
            return updatedHtml;
        }

        internal static string MarkEditableTextFields(string html, ICollection<FieldValue> fieldValues)
        {
            var matchEvaluator = new MatchEvaluator(match =>
            {
                var fieldName = match.Groups[1].Value;
                var fieldValue = fieldValues.FirstOrDefault(fv => fv.FieldName == fieldName);
                if (fieldValue != null)
                {
                    dynamic jsonObject = JObject.Parse(fieldValue.Value);
                    return string.Format("<span class=\"editable-textfield\" data-afvid=\"{0}\">{1}</span>", fieldValue.Id, jsonObject.html);
                }
                return string.Format("<span class=\"editable-textfield\" data-afname=\"{0}\">{0}</span>", fieldName);
            });

            return REGEX_TEXT_FIELDS.Replace(html, matchEvaluator);
        }

        internal static string MarkEditableHtmlFields(string html, ICollection<FieldValue> fieldValues)
        {
            var regex = REGEX_HTML_FIELDS;

            var matchEvaluator = new MatchEvaluator(match =>
            {
                var firstParagraph = match.Groups[1].Value;
                var firstParagraphClass = GetFirstParagraphClass(firstParagraph);
                var fieldName = match.Groups[2].Value;
                var exampleText = match.Groups[4].Value;
                var lastClosingParagraphTag = match.Groups[5].Value;

                var fieldValue = fieldValues.FirstOrDefault(fv => fv.FieldName == fieldName);
                if (fieldValue != null)
                {
                    dynamic jsonObject = JObject.Parse(fieldValue.Value);
                    return string.Format("<div class=\"editable-htmlfield\" data-afvid=\"{0}\" data-firstparagraphclass=\"{1}\">{2}</div>",
                        fieldValue.Id,
                        firstParagraphClass,
                        jsonObject.html);
                }
                return string.Format("<div class=\"editable-htmlfield\" data-afname=\"{0}\" data-firstparagraphclass=\"{1}\">{2}{3}{4}</div>",
                    fieldName,
                    firstParagraphClass,
                    firstParagraph,
                    exampleText,
                    lastClosingParagraphTag);
            });

            return regex.Replace(html, matchEvaluator);
        }

        internal static string MarkEditableImageType1Fields(string html, ICollection<FieldValue> fieldValues)
        {
            var templateString = html;
            var regex = REGEX_IMAGE_FIELDS_TYPE1;
            foreach (Match match in regex.Matches(templateString))
            {
                if (match.Groups.Count > 8)
                {
                    var wrapperTageGroup = match.Groups[1].Value;       // <div id="u397_clip"
                    var idGroup = match.Groups[2].Value;                // id="u397_img" 
                    var srcGroup = match.Groups[3].Value;               // src="images
                    var imageFieldTagGroup = match.Groups[4].Value;     // /bild
                    var imageFieldNameGroup = match.Groups[5].Value;    // 2_3
                    var imageExtensionGroup = match.Groups[6].Value;    // .jpg"
                    var altTagGroup = match.Groups[7].Value;            //  alt=""
                    var widthHeightGroup = match.Groups[8].Value;       //  width="600" height="900"

                    var fieldValue = fieldValues.FirstOrDefault(fv => fv.FieldName == imageFieldNameGroup);
                    if (fieldValue != null)
                    {
                        dynamic jsonObject = JObject.Parse(fieldValue.Value);

                        // Add "editable-imagefield" to parent div ("_clip")
                        // Add img afvid
                        var guillotineData = string.Format("{{\"scale\":{0},\"angle\":{1},\"x\":{2},\"y\":{3}}}",
                            ReplaceDecimalComma(jsonObject.scale), ReplaceDecimalComma(jsonObject.angle), ReplaceDecimalComma(jsonObject.x), ReplaceDecimalComma(jsonObject.y));
                        var htmlGuillotineData = HttpUtility.HtmlEncode(guillotineData);
                        templateString = templateString.Replace(wrapperTageGroup, 
                            wrapperTageGroup + 
                            " class=\"editable-imagefield\"" +
                            " data-imagefieldtype=\"1\"" + 
                            string.Format(" data-afvid=\"{0}\"", fieldValue.Id) + 
                            string.Format(" data-init=\"{0}\"", htmlGuillotineData));

                        // Remove img id tag
                        templateString = templateString.Replace(idGroup, "");

                        // Remove img width/height
                        var currentLastPartWithImageFieldName =
                            imageFieldTagGroup + imageFieldNameGroup + imageExtensionGroup + altTagGroup + widthHeightGroup;
                        templateString = templateString.Replace(
                            currentLastPartWithImageFieldName,
                            imageFieldTagGroup + imageFieldNameGroup + imageExtensionGroup + altTagGroup);

                        // Replace src if field value exists
                        var currentSrc = srcGroup + imageFieldTagGroup + imageFieldNameGroup + imageExtensionGroup;
                        templateString = templateString.Replace(currentSrc, string.Format("src=\"{0}\"", jsonObject.url));
                    }
                    else
                    {
                        // Add "editable-imagefield" to parent div ("_clip")
                        // Add img afname
                        templateString = templateString.Replace(wrapperTageGroup, wrapperTageGroup + " class=\"editable-imagefield\"" + string.Format(" data-imagefieldtype=\"1\" data-afname=\"{0}\"", imageFieldNameGroup));
                    }
                }
            }
            return templateString;
        }

        internal static string MarkEditableImageType2Fields(string html, ICollection<FieldValue> fieldValues)
        {
            var templateString = html;
            foreach (Match match in REGEX_IMAGE_FIELDS_TYPE2.Matches(templateString))
            {
                var wrapperTagFirstPartGroup = match.Groups[1].Value;       // <div class="clip_
                var wrapperTagLastPartGroup = match.Groups[2].Value;        // frame grpelem" id="u163"
                var idGroup = match.Groups[4].Value;                        // id="u397_img" 
                var srcGroup = match.Groups[5].Value;                       // src="images
                var imageFieldTagGroup = match.Groups[6].Value;             // /bild
                var imageFieldNameGroup = match.Groups[7].Value;            // 2_3
                var imageExtensionGroup = match.Groups[8].Value;            // .jpg"
                var altTagGroup = match.Groups[9].Value;                    //  alt=""
                var widthHeightGroup = match.Groups[10].Value;              //  width="600" height="900"
                var remainingLastGroup = match.Groups[11].Value;            //  />

                var fieldValue = fieldValues.FirstOrDefault(fv => fv.FieldName == imageFieldNameGroup);
                if (fieldValue != null)
                {
                    dynamic jsonObject = JObject.Parse(fieldValue.Value);

                    // Add "editable-imagefield" to parent div ("clip_")
                    // Add opening outer div
                    templateString = templateString.Replace(
                        wrapperTagFirstPartGroup + wrapperTagLastPartGroup,
                        "<div><div class=\"editable-imagefield clip_" + wrapperTagLastPartGroup);

                    // Add afvid and data-init to parent div ("clip_")
                    var guillotineData = string.Format("{{\"scale\":{0},\"angle\":{1},\"x\":{2},\"y\":{3}}}",
                        ReplaceDecimalComma(jsonObject.scale), ReplaceDecimalComma(jsonObject.angle), ReplaceDecimalComma(jsonObject.x), ReplaceDecimalComma(jsonObject.y));
                    var htmlGuillotineData = HttpUtility.HtmlEncode(guillotineData);
                    templateString = templateString.Replace(wrapperTagLastPartGroup,
                        wrapperTagLastPartGroup + 
                        " data-imagefieldtype=\"2\"" +
                        string.Format(" data-afvid=\"{0}\"", fieldValue.Id) +
                        string.Format(" data-init=\"{0}\" data-imgurl=\"{1}\"", htmlGuillotineData, jsonObject.url));

                    // Remove img id tag
                    //templateString = templateString.Replace(idGroup, "");

                    // Remove img width/height
                    // Add closing outer div
                    var currentLastPartWithImageFieldName =
                        imageFieldTagGroup + imageFieldNameGroup + imageExtensionGroup + altTagGroup + widthHeightGroup + remainingLastGroup;
                    templateString = templateString.Replace(
                        currentLastPartWithImageFieldName,
                        imageFieldTagGroup + imageFieldNameGroup + imageExtensionGroup + altTagGroup + widthHeightGroup + remainingLastGroup + "</div>");

                    // Replace src if field value exists
                    //var currentSrc = srcGroup + imageFieldTagGroup + imageFieldNameGroup + imageExtensionGroup;
                    //templateString = templateString.Replace(currentSrc, string.Format("src=\"{0}\"", jsonObject.url));
                }
                else
                {
                    // Add "editable-imagefield" to parent div ("clip_")
                    // Add opening outer div
                    templateString = templateString.Replace(
                        wrapperTagFirstPartGroup + wrapperTagLastPartGroup,
                        "<div><div class=\"editable-imagefield clip_" + wrapperTagLastPartGroup);

                    // Add afname to parent div ("clip_")
                    templateString = templateString.Replace(wrapperTagLastPartGroup, wrapperTagLastPartGroup + string.Format(" data-imagefieldtype=\"2\" data-afname=\"{0}\"", imageFieldNameGroup));

                    // Add closing outer div
                    var currentLastPartWithImageFieldName =
                        imageFieldTagGroup + imageFieldNameGroup + imageExtensionGroup + altTagGroup + widthHeightGroup + remainingLastGroup;
                    templateString = templateString.Replace(
                        currentLastPartWithImageFieldName,
                        imageFieldTagGroup + imageFieldNameGroup + imageExtensionGroup + altTagGroup + widthHeightGroup + remainingLastGroup + "</div>");
                }
            }
            return templateString;
        }

        private static string ReplaceDecimalComma(dynamic value)
        {
            return value.ToString().Replace(",", ".");
        }

    }

    public class FieldInfoBase
    {
        public string FieldName { get; set; }
    }
    public class TextFieldInfo : FieldInfoBase
    {
    }
    public class HtmlFieldInfo : FieldInfoBase
    {
        public string FirstParagraphClass { get; set; }
    }
    public class ImageFieldInfo : FieldInfoBase
    {
    }
}
