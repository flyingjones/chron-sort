using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Text.RegularExpressions;

namespace ImageSorter;

public static partial class DateParser
{
    // see https://learn.microsoft.com/en-us/dotnet/api/system.drawing.imaging.propertyitem.id
    private const int OriginalDateTag = 0x9003;
    private const int DigitizedDateTag = 0x9004;
    private const int DateTimeTag = 0x0132;
    private static readonly int CurrentYear = DateTime.Now.Year;

    private static readonly int[] TagsToScan = new[]
    {
        OriginalDateTag,
        DigitizedDateTag,
        DateTimeTag
    };
    
    public static async Task<DateTime?> GetDateTaken(string filePath)
    {
        await using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        try
        {
            using Image myImage = Image.FromStream(fs, false, false);

            foreach (var tag in TagsToScan)
            {
                if (myImage.PropertyIdList.Contains(tag))
                {
                    PropertyItem propItem = myImage.GetPropertyItem(tag);
                    string dateTaken = DotRegex().Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2);
                    var dt = DateTime.Parse(dateTaken);
                    return dt;
                }
            }
        }
        catch (Exception)
        {
            
        }

        var parsedPath = new FileInfo(filePath);
        var fileName = parsedPath.Name;

        var match = SimpleDateInNameRegex().Match(fileName);

        if (match.Success)
        {
            var year = int.Parse(match.Groups[1].Value);
            var month = int.Parse(match.Groups[2].Value);
            var day = int.Parse(match.Groups[3].Value);

            // there shouldn't be any files from the future ...
            if (year <= CurrentYear + 1)
            {
                return new DateTime(year, month, day);
            }
        }

        var lastModifiedDateFromFileSystem = File.GetLastWriteTime(filePath);

        return lastModifiedDateFromFileSystem;
        

        return null;
    }

    [GeneratedRegex(".*(20[0-9]{2}|19[0-9]{2})-?(0[0-9]|1[0-9])-?(0[0-9]|1[0-9]|2[0-9]|3[0-1]).*")]
    private static partial Regex SimpleDateInNameRegex();

    [GeneratedRegex(":")]
    private static partial Regex DotRegex();
}