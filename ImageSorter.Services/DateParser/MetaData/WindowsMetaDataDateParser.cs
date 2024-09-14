using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Services.DateParser.MetaData;

/// <summary>
/// only works on windows -> disable warning that this only works on windows
/// </summary>
#pragma warning disable CA1416
public partial class WindowsMetaDataDateParser : IDateParserImplementation
{
    public int Priority { get; }

    private readonly ExifTagId _exifTagId;
    private readonly ILogger<IDateParserImplementation> _logger;

    public WindowsMetaDataDateParser(ExifTagId exifTagId, int priority, ILogger<IDateParserImplementation> logger)
    {
        _exifTagId = exifTagId;
        Priority = priority;
        _logger = logger;
    }

    public string Name => $"ExifTag:{_exifTagId:G}";

    public bool TryParseDate(ILazyFileMetaDataHandle fileHandle, [NotNullWhen(true)] out DateTime? result)
    {
        result = null;
        if (!ExifTagHelper.FileEndingSupported(fileHandle.FileEnding))
            return false;
        
        var image = fileHandle.GetOrLoadImage();
        if (image == null) return false;

        return TryParseDate(image, out result);
    }

    public bool TryParseDate(Image image, [NotNullWhen(true)] out DateTime? result)
    {
        result = null;
        try
        {
            var property = image.PropertyItems.FirstOrDefault(x => x.Id == (int)_exifTagId);
            if (property != null)
            {
                var propertyValue = Encoding.UTF8.GetString(property.Value ?? Array.Empty<byte>());
                result = MetaDataParserHelpers.ParseDateFromTag(propertyValue);
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
#pragma warning restore CA1416