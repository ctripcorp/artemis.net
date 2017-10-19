using System;
using System.Net;
using System.IO;
namespace Com.Ctrip.Soa.Artemis.Common.Text
{
    public static class WebRequestExtensions
    {
        public const string Json = "application/json";
        public static string PostJsonToUrl(this string url, object data,
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST", requestBody: data.ToJson(), contentType: Json, acceptContentType: Json,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string PostJsonToUrl(this string url, string json,
           Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            return SendStringToUrl(url, method: "POST", requestBody: json, contentType: Json, acceptContentType: Json,
                requestFilter: requestFilter, responseFilter: responseFilter);
        }

        public static string SendStringToUrl(this string url, string method = null,
            string requestBody = null, string contentType = null, string acceptContentType = "*/*",
            Action<HttpWebRequest> requestFilter = null, Action<HttpWebResponse> responseFilter = null)
        {
            var webReq = (HttpWebRequest)WebRequest.Create(url);
            if (method != null)
                webReq.Method = method;
            if (contentType != null)
                webReq.ContentType = contentType;

            webReq.Accept = acceptContentType;
            webReq.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            webReq.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            if (requestFilter != null)
            {
                requestFilter(webReq);
            }

            if (requestBody != null)
            {
                using (var reqStream = webReq.GetRequestStream())
                using (var writer = new StreamWriter(reqStream))
                {
                    writer.Write(requestBody);
                }
            }

            using (var webRes = webReq.GetResponse())
            using (var stream = webRes.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                if (responseFilter != null)
                {
                    responseFilter((HttpWebResponse)webRes);
                }
                return reader.ReadToEnd();
            }
        }
    }
}
