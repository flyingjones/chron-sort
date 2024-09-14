using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace ImageSorter.Services.DateParser.MetaData;

public static class ExifTagHelper
{
    private static readonly Dictionary<ExifTagId, ExifTag<string>> TagDict;
    
    // exif tag support taken from wiki page: https://en.wikipedia.org/wiki/Exif
    private static readonly string[] SupportedFileEndings = new[]{"jpg", "jpeg", "tif", "tiff", "wav", "png", "webp"};

    static ExifTagHelper()
    {
        TagDict = new Dictionary<ExifTagId, ExifTag<string>>
        {
            { ExifTagId.DateTimeOriginal, ExifTag.DateTimeOriginal },
            { ExifTagId.DateTimeDigitized, ExifTag.DateTimeDigitized },
            { ExifTagId.DateTime, ExifTag.DateTime }
        };
    }

    public static ExifTag<string> FromTagId(ExifTagId tagId)
    {
        if (TagDict.TryGetValue(tagId, out var result))
        {
            return result;
        }

        throw new ArgumentException($"Invalid exif tag \"{tagId}\"", nameof(tagId));
    }

    public static bool FileEndingSupported(string fileEnding)
    {
        return SupportedFileEndings.Contains(fileEnding, StringComparer.InvariantCultureIgnoreCase);
    }
}