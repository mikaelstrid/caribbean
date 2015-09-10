using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Caribbean.DataAccessLayer.RealEstateObjects
{
    internal static class Utilities
    {
        // Copied from http://stackoverflow.com/questions/11052744/how-to-efficiently-remove-a-query-string-by-key-from-a-url
        internal static string RemoveQueryStringParamsByKey(string url, IEnumerable<string> keys)
        {
            var uri = new Uri(url);
            //var newQueryString = HttpUtility.ParseQueryString(uri.Query); // Depends on System.Web
            var queryStringParameters = ParseQueryString(uri.Query);
            foreach (var key in keys)
            {
                queryStringParameters.Remove(key);
            }
            var pagePathWithoutQueryString = uri.GetLeftPart(UriPartial.Path);
            return queryStringParameters.Count > 0 ? $"{pagePathWithoutQueryString}?{ToQueryString(queryStringParameters)}" : pagePathWithoutQueryString;
        }

        // http://stackoverflow.com/questions/68624/how-to-parse-a-query-string-into-a-namevaluecollection-in-net
        private static NameValueCollection ParseQueryString(string s)
        {
            var nvc = new NameValueCollection();

            // remove anything other than query string from url
            if (s.Contains("?"))
            {
                s = s.Substring(s.IndexOf('?') + 1);
            }

            foreach (var vp in Regex.Split(s, "&"))
            {
                var singlePair = Regex.Split(vp, "=");
                if (singlePair.Length == 2)
                {
                    nvc.Add(singlePair[0], singlePair[1]);
                }
                else
                {
                    // only one key with no value specified in query string
                    nvc.Add(singlePair[0], string.Empty);
                }
            }

            return nvc;
        }

        private static string ToQueryString(NameValueCollection nvc)
        {
            var parameters = nvc.AllKeys.Select(key => $"{key}={nvc[key]}").ToList();
            return string.Join("&", parameters);
        }
    }
}