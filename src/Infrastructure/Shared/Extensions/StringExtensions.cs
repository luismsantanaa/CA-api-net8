using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Shared.Extensions
{
    public static class StringExtensions
    {
        public static Guid StringToGuid(this string value) => Guid.Parse(value);

        public static string? NormalizeString(this string str)
        {
            if (string.IsNullOrEmpty(str)) return null;

            return str.Replace("\t", "")
                .Replace("\r\n", "")
                .Replace("'", "")
                .Replace("\"", "");

        }
        public static string? NormalizeStringToCompare(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;

            str = str.Replace(" ", "").ToLower().NormalizeString()!.RemoveSpecialCharacters()!;

            return str;
        }
        public static string? RemoveSpecialCharacters(this string str)
        {
            if (string.IsNullOrEmpty(str)) return null;

            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if (c >= '0' && c <= '9' || c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        public static string RemoveAllXmlNamespace(this string xmlData)
        {
            string xmlnsPattern = "\\s+xmlns\\s*(:\\w)?\\s*=\\s*\\\"(?<url>[^\\\"]*)\\\"";
            MatchCollection matchCol = Regex.Matches(xmlData, xmlnsPattern);

            foreach (Match m in matchCol)
            {
                xmlData = xmlData.Replace(m.ToString(), "");
            }
            return xmlData;
        }
        public static string SpanishDateFormat(DateTime date)
        {
            if (string.IsNullOrEmpty(date.ToString())) return DateTime.Now.Date.ToString("ddd, MMM d, yyyy", new CultureInfo("es-ES"));

            return date.ToString("ddd, MMM d, yyyy", new CultureInfo("es-ES"));
        }
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }
        public static int NormalizeInt(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                str = "0";
            }
            return int.Parse(str, CultureInfo.InvariantCulture);
        }
        public static double NormalizeDouble(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                str = "0";
            }
            return double.Parse(str, CultureInfo.InvariantCulture);
        }
        public static decimal NormalizeDecimal(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                str = "0";
            }
            return decimal.Parse(str, CultureInfo.InvariantCulture);
        }
    }
}
