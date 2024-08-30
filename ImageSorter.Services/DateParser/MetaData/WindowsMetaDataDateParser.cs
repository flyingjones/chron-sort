using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Services.DateParser.MetaData;

/// <summary>
/// only works on windows -> disable warning that this only works on windows
/// </summary>
#pragma warning disable CA1416
public class WindowsMetaDataDateParser : IMetaDataDateParser
{
    public int Priority { get; }

    private readonly ExifTagId _exifTagId;
    private readonly ILogger<IMetaDataDateParser> _logger;

    public WindowsMetaDataDateParser(ExifTagId exifTagId, int priority, ILogger<IMetaDataDateParser> logger)
    {
        _exifTagId = exifTagId;
        Priority = priority;
        _logger = logger;
    }

    public bool TryParseDate(FileMetaDataHandle fileMetaDataHandle, [NotNullWhen(true)] out DateTime? result)
    {
        result = null;
        if (fileMetaDataHandle.LoadFailed)
        {
            return false;
        }
        
        try
        {
            var image = fileMetaDataHandle.Image!;

            if (image.PropertyIdList.Contains((int)_exifTagId)) ;
            {
                var propertyItem = image.GetPropertyItem((int)_exifTagId);
                var propertyValue = Encoding.UTF8.GetString(propertyItem?.Value ?? Array.Empty<byte>());
                result = MetaDataParserHelpers.ParseDateFromTag(propertyValue);
                if (result != null) return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogTrace("error while parsing meta data of file {filePath}: {ex}", fileMetaDataHandle.FilePath, ex);
        }

        return false;
    }
}

#pragma warning restore CA1416