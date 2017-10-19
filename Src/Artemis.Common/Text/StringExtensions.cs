using System.Web.Script.Serialization;

namespace Com.Ctrip.Soa.Artemis.Common.Text
{
    public static class StringExtensions
    {
        private static JavaScriptSerializer JsonSerializer = new JavaScriptSerializer();
        public static string ToJson<T>(this T obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        public static T FromJson<T>(this string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
