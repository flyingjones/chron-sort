using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ImageSorter.Services.DateParser.MetaData.ExifTags;

public class ExifTagParser : MetaDataParserBase
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
        const string format = "yyyy:MM:dd HH:mm:ss";
        if (tagDescription != null && DateTime.TryParseExact(tagDescription, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var tmp))
        {
            result = tmp;
            return true;
        }

        return false;
    }
}