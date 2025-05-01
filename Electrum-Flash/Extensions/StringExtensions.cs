using System.Globalization;

namespace Vulpes.Electrum.Domain.Extensions;
public static class StringExtensions
{
    public static string ToTitleCase(this string value) => value.ToTitleCase("en-US");
    public static string ToTitleCase(this string value, string cultureInfoName)
    {
        var textInfo = new CultureInfo(cultureInfoName, false).TextInfo;
        return textInfo.ToTitleCase(value);
    }
}
