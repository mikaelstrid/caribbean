using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using Caribbean.Models.PrintTemplates;
using HtmlAgilityPack;
using Microsoft.WindowsAzure.Storage.Blob;
using NLog;

namespace Caribbean.DataAccessLayer.PrintTemplates
{
    public interface ITemplateMetadataRepository
    {
        IEnumerable<PrintTemplateMetadata> GetAllPrintTemplatesForAgency(string agencySlug);
        PrintTemplateMetadata GetPrintVariantBySlug(string agencySlug, string printVariantSlug);
        PageTemplateMetadata GetPageTemplateBySlug(string agencySlug, string pageTemplateSlug);
    }

    public class TemplateMetadataRepository : TemplateRepositoryBase, ITemplateMetadataRepository
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly bool _disableCaching;
        private const string CACHE_PREFIX_PRINT_VARIANT = "PV_";
        private const string CACHE_PREFIX_PAGE_TEMPLATE_METADATA = "PT_";

        public TemplateMetadataRepository()
        {
            if (!bool.TryParse(ConfigurationManager.AppSettings["Caribbean.PrintTemplates.DisableCaching"], out _disableCaching))
            {
                _disableCaching = false;
            }
        }

        public IEnumerable<PrintTemplateMetadata> GetAllPrintTemplatesForAgency(string agencySlug)
        {
            var cache = MemoryCache.Default;

            var cachedMetadata = cache[CACHE_PREFIX_PRINT_VARIANT + agencySlug];
            if (cachedMetadata != null)
            {
                Logger.Trace($"Print template metadatas for {agencySlug} found in cache.");
                return cachedMetadata as IEnumerable<PrintTemplateMetadata>;
            }

            Logger.Trace($"Print template metadatas for {agencySlug} not in cache.");

            var container = GetContainer("templates");
            if (container == null) return new PrintTemplateMetadata[0];
            var agencyDirectory = container.GetDirectoryReference(agencySlug);
            var createdMetadata = agencyDirectory.ListBlobs()
                .Where(b => b is CloudBlockBlob)
                .Cast<CloudBlockBlob>()
                .Where(b => IsValidMetadataBlob(b.Name, PRINT_VARIANT_PREFIX, PRINT_VARIANT_EXTENSION))
                .Select(CreatePrintVariantMetadataFromBlob);

            if (!_disableCaching)
                cache.Set(CACHE_PREFIX_PRINT_VARIANT + agencySlug, createdMetadata, DateTimeOffset.Now.AddMinutes(30));

            return createdMetadata;
        }

        public PrintTemplateMetadata GetPrintVariantBySlug(string agencySlug, string printVariantSlug)
        {
            var allMetadata = GetAllPrintTemplatesForAgency(agencySlug);
            return allMetadata?.FirstOrDefault(m => m.Slug == printVariantSlug);
        }

        public PageTemplateMetadata GetPageTemplateBySlug(string agencySlug, string pageTemplateSlug)
        {
            var cache = MemoryCache.Default;
            var cacheKey = CACHE_PREFIX_PAGE_TEMPLATE_METADATA + agencySlug + "_" + pageTemplateSlug;

            var cachedMetadata = cache[cacheKey];
            if (cachedMetadata != null)
            {
                Logger.Trace($"Page template metadata for {agencySlug}.{pageTemplateSlug} found in cache.");
                return cachedMetadata as PageTemplateMetadata;
            }

            Logger.Trace($"Page template metadata for {agencySlug}.{pageTemplateSlug} not in cache.");

            var container = GetContainer("templates");
            if (container == null) return null;
            var agencyDirectory = container.GetDirectoryReference(agencySlug);
            var foundPageTemplateBlob = agencyDirectory.ListBlobs()
                .Where(b => b is CloudBlockBlob)
                .Cast<CloudBlockBlob>()
                .SingleOrDefault(b => IsMatchingPageTemplate(b.Name, pageTemplateSlug));

            if (foundPageTemplateBlob == null)
            {
                Logger.Warn($"No page template blob for {agencySlug}.{pageTemplateSlug} could be found.");
                return null;
            }
            var createdMetadata = CreatePageTemplateMetadataFromBlob(foundPageTemplateBlob);

            if (!_disableCaching)
                cache.Set(cacheKey, createdMetadata, DateTimeOffset.Now.AddMinutes(30));

            return createdMetadata;
        }


        //=========================================================================================
        // COMMON HELPER FUNCTIONS
        //=========================================================================================
        private static PrintTemplateMetadata CreatePrintVariantMetadataFromBlob(CloudBlockBlob blob)
        {
            var printVariantHtml = DownloadHtmlDocument(blob);
            if (printVariantHtml == null) return PrintTemplateMetadata.CreateInvalid("The print variant HTML could not be downloaded or parsed.");
            return ParseHtmlIntoPrintVariantMetadata(printVariantHtml, blob);
        }

        private static PageTemplateMetadata CreatePageTemplateMetadataFromBlob(ICloudBlob blob)
        {
            var pageTemplateHtml = DownloadHtmlDocument(blob);
            if (pageTemplateHtml == null) return PageTemplateMetadata.CreateInvalid("The page template HTML could not be downloaded or parsed.");
            return ParseHtmlIntoPageTemplateMetadata(pageTemplateHtml, blob);
        }


        // === NAME / SLUG METHODS ===
        internal const string PRINT_VARIANT_PREFIX = "t-";
        internal const string PRINT_VARIANT_EXTENSION = ".html";
        internal const string PRINT_VARIANT_THUMBNAIL_EXTENSION = ".png";
        internal const string PAGE_TEMPLATE_PREFIX = "";

        internal static bool IsValidMetadataBlob(string name, string prefix, string extension)
        {
            var lastPart = GetLastPart(name);
            return lastPart.StartsWith(PRINT_VARIANT_PREFIX) && lastPart.EndsWith(PRINT_VARIANT_EXTENSION);
        }
        internal static string GetPrintVariantThumbnailUrl(Uri uri)
        {
            return Path.ChangeExtension(uri.ToString(), PRINT_VARIANT_THUMBNAIL_EXTENSION);
        }

        internal static string GetSlug(string name, string prefix, string extension)
        {
            var lastPart = GetLastPart(name);
            return IsValidMetadataBlob(name, prefix, extension)
                ? lastPart.Substring(prefix.Length).Substring(0, lastPart.Length - prefix.Length - extension.Length)
                : null;
        }


        // === HTML DOCUMENT METHODS ===
        private static HtmlDocument DownloadHtmlDocument(ICloudBlob blob)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    blob.DownloadToStream(memoryStream);
                    memoryStream.Position = 0;
                    var htmlDocument = new HtmlDocument();
                    htmlDocument.Load(memoryStream);
                    return htmlDocument;
                }
            }
            catch (Exception e)
            {
                Logger.Warn(e, $"Error downloading Html document {blob.StorageUri}.");
                return null;
            }
        }

        private static PrintTemplateMetadata ParseHtmlIntoPrintVariantMetadata(HtmlDocument html, ICloudBlob blob)
        {
            try
            {
                var metaNodes = html.DocumentNode.SelectNodes("//meta");
                return new PrintTemplateMetadata
                    {
                        Slug = GetSlug(blob.Name, PRINT_VARIANT_PREFIX, PRINT_VARIANT_EXTENSION),
                        StorageUri = blob.StorageUri.ToString(),
                        Type = GetMetaContent("typ", metaNodes),
                        Name = GetMetaContent("namn", metaNodes),
                        Description = GetMetaContent("description", metaNodes),
                        ThumbnailUrl = GetPrintVariantThumbnailUrl(blob.Uri),
                        AvailablePageTemplateSlugs = GetMetaContent("tillgangligaSidMallar", metaNodes).Split(';'),
                        ProposedPageSlugs = GetMetaContent("foreslagnaSidor", metaNodes).Split(';'),
                    };
            }
            catch (Exception e)
            {
                Logger.Warn(e, $"Error parsing HTML into print variant metadata {blob.StorageUri}.");
                return PrintTemplateMetadata.CreateInvalid("The HTML is invalid.", blob.StorageUri.ToString());
            }
        }

        private static PageTemplateMetadata ParseHtmlIntoPageTemplateMetadata(HtmlDocument html, ICloudBlob blob)
        {
            try
            {
                var metaNodes = html.DocumentNode.SelectNodes("//meta");
                int dpi;
                return new PageTemplateMetadata
                {
                    Slug = GetSlug(blob.Name, PAGE_TEMPLATE_PREFIX, PAGE_TEMPLATE_EXTENSION),
                    StorageUri = blob.StorageUri.ToString(),
                    Name = GetMetaContent("namn", metaNodes),
                    Description = GetMetaContent("description", metaNodes),
                    ThumbnailUrl = GetPrintVariantThumbnailUrl(blob.Uri),
                    Width = int.Parse(GetMetaContent("bredd", metaNodes)),
                    Height = int.Parse(GetMetaContent("hojd", metaNodes)),
                    Dpi = int.TryParse(GetMetaContent("dpi", metaNodes), out dpi) ? dpi : 300
                };
            }
            catch (Exception e)
            {
                Logger.Warn(e, $"Error parsing HTML into page template metadata {blob.StorageUri}.");
                return PageTemplateMetadata.CreateInvalid("The HTML is invalid.", blob.StorageUri.ToString());
            }
        }

        private static string GetMetaContent(string name, IEnumerable<HtmlNode> metaNodes)
        {
            var node = metaNodes.FirstOrDefault(n => n.GetAttributeValue("name", "") == name);
            if (node == null) return null;
            return Encoding.UTF8.GetString(Encoding.Default.GetBytes(node.GetAttributeValue("content", null)));
        }
    }
}