using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Caribbean.Models.Database;
using Newtonsoft.Json;

namespace Caribbean.Aruba.Web.Business
{
    public interface IPrintPageHtmlStringFactory
    {
        string CreatePageEditorHtmlString(string templateHtml, Page modelPage);
        string CreatePageRenderHtmlString(string templateHtml, Page modelPage);
    }

    public class PrintPageHtmlStringFactory : IPrintPageHtmlStringFactory
    {
        private readonly IMuseTemplateParser _museTemplateParser;

        public PrintPageHtmlStringFactory(IMuseTemplateParser museTemplateParser)
        {
            _museTemplateParser = museTemplateParser;
        }

        public string CreatePageEditorHtmlString(string templateHtml, Page modelPage)
        {
            var editorHtmlString = _museTemplateParser.MarkAllFields(templateHtml, modelPage.FieldValues);
            editorHtmlString = _museTemplateParser.ReplaceInvalidElements(editorHtmlString);
            editorHtmlString = InjectPageIdInHtmlTag(editorHtmlString, modelPage.Id);
            editorHtmlString = InjectStyles(editorHtmlString, new[] { "/Stylesheets/vendor/jquery.guillotine.css", "/Stylesheets/main-editor.css" });
            return editorHtmlString;
        }

        public string CreatePageRenderHtmlString(string templateHtml, Page modelPage)
        {
            var renderHtmlString = _museTemplateParser.MarkAllFields(templateHtml, modelPage.FieldValues);
            renderHtmlString = _museTemplateParser.ReplaceInvalidElements(renderHtmlString);
            renderHtmlString = InjectPageIdInHtmlTag(renderHtmlString, modelPage.Id);
            renderHtmlString = InjectStyles(renderHtmlString, new[] { "/Stylesheets/vendor/jquery.guillotine.css", "/Stylesheets/main-renderer.css" });
            renderHtmlString = InjectScripts(renderHtmlString, new[] { "/Scripts/vendor/jquery.guillotine.js", "/Scripts/lib/lodash.js", "/Scripts/render-scripts.js" });
            renderHtmlString = InjectFieldValues(renderHtmlString, modelPage);
            return renderHtmlString;
        }


        internal static string InjectPageIdInHtmlTag(string html, int pageId)
        {
            var regex = new Regex("(<html.*?)>");
            return regex.Replace(html, "$1 data-pageid=\"" + pageId + "\">");
        }

        internal static string InjectStyles(string html, IEnumerable<string> styleSheetUrls)
        {
            var regex = new Regex("(<\\/head>)");
            var templateString = html;
            foreach (var styleSheetUrl in styleSheetUrls)
            {
                var styleSheetLink = $"    <link href=\"{styleSheetUrl}\" rel=\"stylesheet\" />\r\n";
                templateString = regex.Replace(templateString, styleSheetLink + "$1");
            }
            return templateString;
        }

        internal static string InjectScripts(string html, IEnumerable<string> scriptUrls)
        {
            var regex = new Regex("(<\\/body>)");
            var templateString = html;
            foreach (var scriptUrl in scriptUrls)
            {
                var scriptLink = $"    <script src=\"{scriptUrl}\"></script>\r\n";
                templateString = regex.Replace(templateString, scriptLink + "$1");
            }
            return templateString;
        }

        internal static string InjectFieldValues(string html, Page modelPage)
        {
            var fieldValues = modelPage.FieldValues.Select(fv => new FieldValueViewModel { id = fv.Id, name = fv.FieldName, value = fv.Value });
            var fieldValuesJsonString = JsonConvert.SerializeObject(fieldValues);
            var regex = new Regex("(<\\/head>)");
            var fieldValuesScript = $"    <script>var fieldValues={fieldValuesJsonString};</script>\r\n";
            return regex.Replace(html, fieldValuesScript + "$1");
        }


        internal class FieldValueViewModel
        {
            public int id { get; set; }
            public string name { get; set; }
            public string value { get; set; }
        }
    }
}