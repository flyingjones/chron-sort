using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

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
        IMetaDataTagWrapper metaDataWrapper,
        [NotNullWhen(true)] out DateTime? result)
    {
        result = null;
        var tagDescription = metaDataWrapper.GetExifTagValue(_exifTagId);
        if (tagDescription != null && TryParseDateFromExifTag(tagDescription, out var tmp))
        {
            result = tmp;
            return true;
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