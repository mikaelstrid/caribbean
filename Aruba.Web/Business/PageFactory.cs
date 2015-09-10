using System.Collections.Generic;
using System.Text.RegularExpressions;
using Caribbean.Models.Database;

namespace Caribbean.Aruba.Web.Business
{
    public interface IPageFactory
    {
        string CreatePageEditorHtmlString(string templateHtml, Page modelPage);
        string CreatePageRenderHtmlString(string templateHtml, Page modelPage);
    }

    public class PageFactory : IPageFactory
    {
        private readonly IMuseTemplateParser _museTemplateParser;

        public PageFactory(IMuseTemplateParser museTemplateParser)
        {
            _museTemplateParser = museTemplateParser;
        }

        public string CreatePageEditorHtmlString(string templateHtml, Page modelPage)
        {
            var editorHtmlString = _museTemplateParser.MarkAllFields(templateHtml, modelPage.FieldValues);
            editorHtmlString = InjectPageIdInHtmlTag(editorHtmlString, modelPage.Id);
            editorHtmlString = InjectStyles(editorHtmlString, new[] { "/Content/thirdparty/jquery.guillotine.css", "/Content/pixel/editor-styles.css" });
            return editorHtmlString;
        }

        public string CreatePageRenderHtmlString(string templateHtml, Page modelPage)
        {
            var renderHtmlString = _museTemplateParser.MarkAllFields(templateHtml, modelPage.FieldValues);
            renderHtmlString = InjectPageIdInHtmlTag(renderHtmlString, modelPage.Id);
            renderHtmlString = InjectStyles(renderHtmlString, new[] { "/Content/thirdparty/jquery.guillotine.css", "/Content/pixel/render-styles.css" });
            renderHtmlString = InjectScripts(renderHtmlString, new[] { "/Scripts/thirdparty/jquery.guillotine.js", "/Scripts/pixel/render-scripts.js" });
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
                var styleSheetLink = string.Format("    <link href=\"{0}\" rel=\"stylesheet\" />\r\n", styleSheetUrl);
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
                var scriptLink = string.Format("    <script src=\"{0}\"></script>\r\n", scriptUrl);
                templateString = regex.Replace(templateString, scriptLink + "$1");
            }
            return templateString;
        }
    }
}