using System.Text.RegularExpressions;

namespace ImageSorter.Services.DateParser.MetaData;

public static partial class MetaDataParserHelpers
{
    public static DateTime? ParseDateFromTag(string dateString)
    {
        var cleanedUpDate = DotRegex().Replace(dateString, "-", 2);
        if (DateTime.TryParse(cleanedUpDate, out var result))
        {
            return result;
        }

        return null;
    }
    
    [GeneratedRegex(":")]
    private static partial Regex DotRegex();
}