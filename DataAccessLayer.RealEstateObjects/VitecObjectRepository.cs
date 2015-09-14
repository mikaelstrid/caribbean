using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Text;
using System.Xml.Linq;
using Caribbean.Models.RealEstateObjects;

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
        private const string CACHE_PREFIX_SUMMARY = "VS_";
        private const string CACHE_PREFIX_DETAILS = "VD_";
        private const string VITECT_CURRENT_OBJECT_LIST_URL_WITH_FORMAT = "http://net.sfd.se/webpack/ObjectList/ObjectList.aspx?RenderAsXML=1&Custom=1&DBSPace={0}";
        private const string VITECT_REFERENCE_OBJECT_LIST_URL_WITH_FORMAT = "http://net.sfd.se/Gateway.aspx?SFDGatewayID=58&DBSPace={0}&RefObject=3";

        private readonly IVitecObjectFactory _vitecObjectFactory;

        public VitecObjectRepository(IVitecObjectFactory vitecObjectFactory)
        {
            _vitecObjectFactory = vitecObjectFactory;
        }

        public IEnumerable<VitecObjectSummary> GetAllSummariesForAgency(string vitecCustomerId)
        {
            var cache = MemoryCache.Default;

            var cachedObjectSummaries = cache[CACHE_PREFIX_SUMMARY + vitecCustomerId];
            if (cachedObjectSummaries != null) return cachedObjectSummaries as IEnumerable<VitecObjectSummary>;

            var createdObjectSummaries = GetSummariesAndUpdateCache(vitecCustomerId, cache);

            return createdObjectSummaries;
        }

        public VitecObjectSummary GetSummaryById(string vitecCustomerId, string objectId)
        {
            var cache = MemoryCache.Default;

            var cachedObjectSummaries = cache[CACHE_PREFIX_SUMMARY + vitecCustomerId];
            var matchingSummary = ((IEnumerable<VitecObjectSummary>) cachedObjectSummaries)?.FirstOrDefault(s => s.Id == objectId);
            if (matchingSummary != null) return matchingSummary;

            var createdObjectSummaries = GetSummariesAndUpdateCache(vitecCustomerId, cache);

            return createdObjectSummaries.FirstOrDefault(s => s.Id == objectId);
        }

        private IEnumerable<VitecObjectSummary> GetSummariesAndUpdateCache(string vitecCustomerId, MemoryCache cache)
        {
            var result = new List<VitecObjectSummary>();
            result.AddRange(CreateSummaries(LoadVitecSummaryXmlDocument(vitecCustomerId, VITECT_CURRENT_OBJECT_LIST_URL_WITH_FORMAT)));
            result.AddRange(CreateSummaries(LoadVitecSummaryXmlDocument(vitecCustomerId, VITECT_REFERENCE_OBJECT_LIST_URL_WITH_FORMAT)));
            if (result.Any())
            {
                cache.Set(CACHE_PREFIX_SUMMARY + vitecCustomerId, result, DateTimeOffset.Now.AddMinutes(30));
            }
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

        private IEnumerable<VitecObjectSummary> CreateSummaries(XDocument xmlDocument)
        {
            if (xmlDocument != null)
            {
                return xmlDocument.Descendants("objekt").Select(_vitecObjectFactory.CreateSummary);
            }
            return new VitecObjectSummary[0];
        }

        


        public VitecObjectDetails GetDetailsById(string objectId)
        {
            var cache = MemoryCache.Default;

            var cachedObjectDetails = cache[CACHE_PREFIX_DETAILS + objectId];
            if (cachedObjectDetails != null) return cachedObjectDetails as VitecObjectDetails;

            var xml = LoadVitecDetailsXmlString(objectId);
            if (xml == null) return null;
            var createdObjectDetails = _vitecObjectFactory.CreateDetails(xml);
            cache.Set(CACHE_PREFIX_DETAILS + objectId, createdObjectDetails, DateTimeOffset.Now.AddMinutes(30));
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