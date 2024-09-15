using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using MetadataExtractor.Formats.QuickTime;
using Directory = MetadataExtractor.Directory;

namespace ImageSorter.Services.DateParser.MetaData.QuickTimeMovieHeaders;

public class QuickTimeMovieHeaderParser : MetaDataParserBase
{
    private static readonly string[] SupportedFileEndingsInternal = { "mp4", "qt", "mov" };
    private static readonly DateTime DefaultDate = DateTime.Parse("1904-01-01").Date;
    private readonly QuickTimeMovieHeader _header;

    public QuickTimeMovieHeaderParser(QuickTimeMovieHeader header, int priority) : base(priority)
    {
        _header = header;
    }

    public override string Name => $"QuickTimeMovieHeader:{_header:G}";
    protected override IEnumerable<string> SupportedFileEndings() => SupportedFileEndingsInternal;

    protected override bool TryParseMetaDataDirectories(
        IMetaDataTagWrapper metaDataWrapper,
        [NotNullWhen(true)] out DateTime? result)
    {
        result = null;
        
        var tagDescription = metaDataWrapper.GetQuickTimeMovieHeaderValue(_header);
        return tagDescription != null && TryParseTagDescription(tagDescription, out result);
    }
    
    private static bool TryParseTagDescription(string description, [NotNullWhen(true)] out DateTime? result)
    {
        result = null;
        const string format = "ddd MMM dd HH:mm:ss yyyy";
        
        // Try to parse the date string
        if (DateTime.TryParseExact(description, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
        {
            // minimum date is probably the default value and not useful
            if (parsedDate.Date == DefaultDate)
                return false;
            
            result = parsedDate;
            return true;
        }

        return false;
    }
}