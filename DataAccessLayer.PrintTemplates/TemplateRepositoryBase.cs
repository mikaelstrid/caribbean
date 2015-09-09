using System.Linq;

namespace Caribbean.DataAccessLayer.PrintTemplates
{
    public abstract class TemplateRepositoryBase : BlobStorageRepositoryBase
    {
        internal const string PAGE_TEMPLATE_EXTENSION = ".html";

        internal static bool IsMatchingPageTemplate(string blobName, string pageTemplateSlug)
        {
            return blobName.EndsWith(pageTemplateSlug + PAGE_TEMPLATE_EXTENSION);
        }

        protected static string GetLastPart(string input)
        {
            return string.IsNullOrWhiteSpace(input) ? string.Empty : input.Trim('/').Split('/').Last();
        }
    }
}