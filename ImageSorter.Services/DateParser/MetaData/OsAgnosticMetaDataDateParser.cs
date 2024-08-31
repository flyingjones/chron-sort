using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;

namespace ImageSorter.Services.DateParser.MetaData;

public partial class OsAgnosticMetaDataDateParser : IDateParserImplementation
{
    public int Priority { get; }

    private readonly ExifTagId _exifTagId;
    private readonly ILogger<IDateParserImplementation> _logger;

    public OsAgnosticMetaDataDateParser(ExifTagId exifTagId, int priority, ILogger<IDateParserImplementation> logger)
    {
        _exifTagId = exifTagId;
        Priority = priority;
        _logger = logger;
    }

    public string Name => $"ExifTag:{_exifTagId:G}";

    public bool TryParseDate(ILazyFileMetaDataHandle fileHandle, [NotNullWhen(true)] out DateTime? result)
    {
        var imageInfo = fileHandle.GetOrLoadImageInfo();
        result = null;
        if (imageInfo == null) return false;

        return TryParseDate(imageInfo, out result);
    }

    public bool TryParseDate(ImageInfo imageInfo, [NotNullWhen(true)] out DateTime? result)
    {
        result = null;
        try
        {
            if (imageInfo.Metadata.ExifProfile == null) return false;

            var mappedTag = ExifTagHelper.FromTagId(_exifTagId);
            if (imageInfo.Metadata.ExifProfile.TryGetValue(mappedTag, out var dateString))
            {
                if (dateString.Value == null)
                {
                    return false;
                }

                result = MetaDataParserHelpers.ParseDateFromTag(dateString.Value);
                if (result != null) return true;
            }
        }
        catch (Exception ex)
        {
            LogMetaDataParsingError(ex);
        }

        return false;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Error while parsing meta data of file")]
    private partial void LogMetaDataParsingError(Exception exception);
}