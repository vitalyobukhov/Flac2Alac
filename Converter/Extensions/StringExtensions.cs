using System.Globalization;

namespace Converter.Extensions
{
    static class StringExtensions
    {
        public static bool Matches(this string oneString, string anotherString)
        {
            return string.Compare(oneString, anotherString, false, CultureInfo.InvariantCulture) == 0;
        }
    }
}
