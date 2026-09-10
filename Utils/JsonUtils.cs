using System.Globalization;
using System.Text.RegularExpressions;

namespace NomisKitchenHDT.Utils
{
    internal static class JsonUtils
    {
        internal static int ParseInt(string json, string key)
        {
            var m = Regex.Match(json, "\"" + Regex.Escape(key) + "\"\\s*:\\s*(-?\\d+)");
            return m.Success ? int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) : 0;
        }

        internal static double ParseDouble(string json, string key)
        {
            var m = Regex.Match(json, "\"" + Regex.Escape(key) + "\"\\s*:\\s*(-?\\d+(?:\\.\\d+)?)");
            return m.Success ? double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) : 0;
        }

        internal static bool ParseBool(string json, string key)
        {
            var m = Regex.Match(json, "\"" + Regex.Escape(key) + "\"\\s*:\\s*(true|false)");
            return m.Success && m.Groups[1].Value == "true";
        }
    }
}
