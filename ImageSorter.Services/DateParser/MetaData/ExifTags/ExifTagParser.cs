using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using MetadataExtractor.Formats.Exif;
using Directory = MetadataExtractor.Directory;

namespace ImageSorter.Services.DateParser.MetaData.ExifTags;

public partial class ExifTagParser : MetaDataParserBase
{
    private static readonly IReadOnlyCollection<string> SupportedFileEndingsInternal =
        new ReadOnlyCollection<string>(new[] { "jpg", "jpeg", "tif", "tiff", "wav", "png", "webp" });

    private readonly ExifTagId _exifTagId;

    public ExifTagParser(ExifTagId exifTagId, int priority) : base(priority)
    {
        _exifTagId = exifTagId;
    }

    public override string Name => $"ExifTag:{_exifTagId:G}";

    protected override IEnumerable<string> SupportedFileEndings() => SupportedFileEndingsInternal;

    protected override bool TryParseMetaDataDirectories(
        IReadOnlyList<Directory> directories,
        [NotNullWhen(true)] out DateTime? result)
    {
        result = null;
        var tags = directories
            .Where(x => x is ExifIfd0Directory or ExifSubIfdDirectory)
            .SelectMany(x => x.Tags);
        var tagDescription = tags.FirstOrDefault(tag => tag.Type == (int)_exifTagId)?.Description;

        if (tagDescription != null)
        {
            if (TryParseDateFromExifTag(tagDescription, out var tmp))
            {
                result = tmp;
                return true;
            }
        }

        return false;
    }
    
    private static bool TryParseDateFromExifTag(string dateString, out DateTime result)
    {
        var cleanedUpDate = DotRegex().Replace(dateString, "-", 2);
        return DateTime.TryParse(cleanedUpDate, out result);
    }
    
    [GeneratedRegex(":")]
    private static partial Regex DotRegex();
}