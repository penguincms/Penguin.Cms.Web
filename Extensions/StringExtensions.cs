using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Penguin.Cms.Web.Extensions
{
    public static class StringExtensions
    {
        public static string PrettifyJson(this string stringValue) => JValue.Parse(stringValue).ToString(Formatting.Indented);
    }
}