using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Caribbean.DataAccessLayer.PrintTemplates
{
    public interface ITemplateContentRepository
    {
        string GetPageTemplateBySlug(string agencySlug, string pageTemplateSlug);
        void EmptyCache();
    }

    public class TemplateContentRepository : TemplateRepositoryBase, ITemplateContentRepository
    {
        private readonly bool _disableCaching;


        public TemplateContentRepository()
        {
            if (!bool.TryParse(ConfigurationManager.AppSettings["Caribbean.PrintTemplates.DisableCaching"], out _disableCaching))
            {
                _disableCaching = false;
            }
        }

        private const string CACHE_PREFIX_PAGE_TEMPLATE_CONTENT = "PTC_";


        public string GetPageTemplateBySlug(string agencySlug, string pageTemplateSlug)
        {
            var cache = MemoryCache.Default;
            var cacheKey = CACHE_PREFIX_PAGE_TEMPLATE_CONTENT + agencySlug + "_" + pageTemplateSlug;

            var cachedMetadata = cache[cacheKey];
            if (cachedMetadata != null) return cachedMetadata as string;

            var container = GetContainer("templates");
            if (container == null) return null;

            var agencyDirectory = container.GetDirectoryReference(agencySlug);
            var foundPageTemplateBlob = agencyDirectory.ListBlobs()
                .Where(b => b is CloudBlockBlob)
                .Cast<CloudBlockBlob>()
                .SingleOrDefault(b => IsMatchingPageTemplate(b.Name, pageTemplateSlug));
            if (foundPageTemplateBlob == null) return null;

            var subFolders = agencyDirectory.ListBlobs()
                .Where(b => b is CloudBlobDirectory)
                .Cast<CloudBlobDirectory>()
                .Select(d => GetLastPart(d.Prefix));

            var templateString = DownloadString(foundPageTemplateBlob);
            if (string.IsNullOrWhiteSpace(templateString)) return null;

            templateString = ReplaceTemplatePaths(templateString, foundPageTemplateBlob.Parent.Uri.ToString(), FindAllRelativePaths(templateString, subFolders));

            if (!_disableCaching)
                cache.Set(cacheKey, templateString, DateTimeOffset.Now.AddMinutes(30));

            return templateString;
        }

        private static string DownloadString(ICloudBlob blob)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    blob.DownloadToStream(memoryStream);
                    return System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                }
            }
            catch
            {
                return null;
            }
        }

        private static string ReplaceTemplatePaths(string templateString, string baseUrl, IEnumerable<string> relativePaths)
        {
            foreach (var path in relativePaths)
            {
                templateString = templateString.Replace(path, baseUrl + path);
            }
            return templateString;
        }
        
        internal static IEnumerable<string> FindAllRelativePaths(string html, IEnumerable<string> subFolders)
        {
            var result = new List<string>();
            foreach (var subFolder in subFolders)
            {
                var regex = new Regex($"\"({subFolder}\\/.*?)\"");
                foreach (Match match in regex.Matches(html))
                {
                    if (match.Groups.Count > 1)
                    {
                        var value = match.Groups[1].Value;
                        if (!result.Contains(value))
                            result.Add(value);
                    }
                }
            }
            return result;
        }


        public void EmptyCache()
        {
            var cache = MemoryCache.Default;
            foreach (var entry in cache.Where(e => e.Key.StartsWith(CACHE_PREFIX_PAGE_TEMPLATE_CONTENT)))
            {
                cache.Remove(entry.Key);
            }
        }
    }
}