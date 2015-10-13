using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Text;
using System.Xml.Linq;
using Caribbean.Models.RealEstateObjects;
using NLog;

namespace Caribbean.DataAccessLayer.RealEstateObjects
{
    public interface IVitecObjectRepository
    {
        IEnumerable<VitecObjectSummary> GetAllSummariesForAgency(string vitecCustomerId);
        VitecObjectSummary GetSummaryById(string vitecCustomerId, string objectId);
        VitecObjectDetails GetDetailsById(string objectId);
    }

    public class VitecObjectRepository : IVitecObjectRepository
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string CACHE_PREFIX_SUMMARY = "VS_";
        private const string CACHE_PREFIX_DETAILS = "VD_";
        private const string CACHE_PREFIX_MODIFIED_TIME = "VMT_";
        //private const string VITECT_CURRENT_OBJECT_LIST_URL_WITH_FORMAT = "http://net.sfd.se/webpack/ObjectList/ObjectList.aspx?RenderAsXML=1&Custom=1&DBSPace={0}";
        private const string VITECT_CURRENT_AND_COMING_OBJECT_LIST_URL_WITH_FORMAT = "http://net.sfd.se/Gateway.aspx?SFDGatewayID=58&DBSPace={0}&KommandeForsaljningar=2";
        private const string VITECT_REFERENCE_OBJECT_LIST_URL_WITH_FORMAT = "http://net.sfd.se/Gateway.aspx?SFDGatewayID=58&DBSPace={0}&RefObject=3";

        private readonly IVitecObjectFactory _vitecObjectFactory;
        private readonly bool _disableCaching;

        public VitecObjectRepository(IVitecObjectFactory vitecObjectFactory)
        {
            _vitecObjectFactory = vitecObjectFactory;
            if (!bool.TryParse(ConfigurationManager.AppSettings["Caribbean.RealEstateObjects.DisableCaching"], out _disableCaching))
            {
                _disableCaching = false;
            }
        }

        public IEnumerable<VitecObjectSummary> GetAllSummariesForAgency(string vitecCustomerId)
        {
            var cache = MemoryCache.Default;

            var cachedObjectSummaries = cache[CACHE_PREFIX_SUMMARY + vitecCustomerId];
            if (cachedObjectSummaries != null)
            {
                Logger.Trace($"Object summaries for {vitecCustomerId} found in cache.");
                return cachedObjectSummaries as IEnumerable<VitecObjectSummary>;
            }

            Logger.Trace($"Object summaries for {vitecCustomerId} not in cache.");
            var createdObjectSummaries = GetSummariesAndUpdateCache(vitecCustomerId, cache);

            return createdObjectSummaries;
        }

        public VitecObjectSummary GetSummaryById(string vitecCustomerId, string objectId)
        {
            var cache = MemoryCache.Default;

            var cachedObjectSummaries = cache[CACHE_PREFIX_SUMMARY + vitecCustomerId];
            var matchingSummary = ((IEnumerable<VitecObjectSummary>) cachedObjectSummaries)?.FirstOrDefault(s => s.Id == objectId);
            if (matchingSummary != null)
            {
                Logger.Trace($"Object summary {objectId} for {vitecCustomerId} found in cache.");
                return matchingSummary;
            }

            Logger.Trace($"Object summary {objectId} for {vitecCustomerId} not in cache.");
            var createdObjectSummaries = GetSummariesAndUpdateCache(vitecCustomerId, cache);

            return createdObjectSummaries.FirstOrDefault(s => s.Id == objectId);
        }

        private IEnumerable<VitecObjectSummary> GetSummariesAndUpdateCache(string vitecCustomerId, MemoryCache cache)
        {
            var result = new List<VitecObjectSummary>();
            result.AddRange(CreateSummaries(cache, LoadVitecSummaryXmlDocument(vitecCustomerId, VITECT_CURRENT_AND_COMING_OBJECT_LIST_URL_WITH_FORMAT)));
            result.AddRange(CreateSummaries(cache, LoadVitecSummaryXmlDocument(vitecCustomerId, VITECT_REFERENCE_OBJECT_LIST_URL_WITH_FORMAT)));

            if (result.Any() && !_disableCaching)
                cache.Set(CACHE_PREFIX_SUMMARY + vitecCustomerId, result, DateTimeOffset.Now.AddMinutes(30));

            return result;
        }

        private static XDocument LoadVitecSummaryXmlDocument(string vitecCustomerId, string objectListUrlWithFormat)
        {
            try
            {
                var objectListUrl = string.Format(objectListUrlWithFormat, vitecCustomerId);
                return XDocument.Load(objectListUrl);
            }
            catch
            {
                return null;
            }
        }

        private IEnumerable<VitecObjectSummary> CreateSummaries(MemoryCache cache, XDocument xmlDocument)
        {
            if (xmlDocument != null)
            {
                return xmlDocument.Descendants("objekt").Select(o => CreateSummary(cache, o));
            }
            return new VitecObjectSummary[0];
        }

        private VitecObjectSummary CreateSummary(MemoryCache cache, XElement vitecObjectXml)
        {
            var createdSummary = _vitecObjectFactory.CreateSummary(vitecObjectXml);

            try
            {
                var valueInCache = cache[CACHE_PREFIX_MODIFIED_TIME + createdSummary.Id];
                if (valueInCache != null)
                {
                    var cachedModifiedTime = (DateTime) valueInCache;
                    createdSummary.CreatedTime = cachedModifiedTime;
                }
                else
                {
                    var details = GetDetailsById(createdSummary.Id);
                    createdSummary.CreatedTime = details.CreatedTime;
                }
            }
            catch
            {
                var details = GetDetailsById(createdSummary.Id);
                createdSummary.CreatedTime = details.CreatedTime;
            }
            return createdSummary;
        }


        public VitecObjectDetails GetDetailsById(string objectId)
        {
            var cache = MemoryCache.Default;

            var cachedObjectDetails = cache[CACHE_PREFIX_DETAILS + objectId];
            if (cachedObjectDetails != null)
            {
                Logger.Trace($"Object details {objectId} found in cache.");
                return cachedObjectDetails as VitecObjectDetails;
            }

            Logger.Trace($"Object details {objectId} not in cache.");

            var xml = LoadVitecDetailsXmlString(objectId);
            if (xml == null)
            {
                Logger.Warn($"XML for object {objectId} could not be downloaded.");
                return null;
            }

            var createdObjectDetails = _vitecObjectFactory.CreateDetails(xml);

            cache.Set(CACHE_PREFIX_MODIFIED_TIME + objectId, createdObjectDetails.CreatedTime, DateTimeOffset.Now.AddMonths(1));
            if (!_disableCaching)
            {
                cache.Set(CACHE_PREFIX_DETAILS + objectId, createdObjectDetails, DateTimeOffset.Now.AddMinutes(30));
            }

            return createdObjectDetails;
        }

        private static string LoadVitecDetailsXmlString(string objectId)
        {
            try
            {
                const string VITEC_OBJECT_URL = "http://w4.objektdata.se/pregen/{0}/{1}/wp.xml";
                var url = string.Format(VITEC_OBJECT_URL, objectId.Substring(objectId.Length - 3), objectId);
                var webClient = new WebClient { Encoding = Encoding.GetEncoding("iso-8859-1") };
                return webClient.DownloadString(url);
            }
            catch
            {
                return null;
            }
        }
    }
}