using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Services.DateParser.MetaData;

public class OsAgnosticMetaDataDateParser : IMetaDataDateParser
{
    public int Priority { get; }

    private readonly ExifTagId _exifTagId;
    private readonly ILogger<IMetaDataDateParser> _logger;

    public OsAgnosticMetaDataDateParser(ExifTagId exifTagId, int priority, ILogger<IMetaDataDateParser> logger)
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
            var imageInfo = fileMetaDataHandle.ImageInfo!;


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
            _logger.LogTrace("error while parsing meta data of file {filePath}: {ex}", fileMetaDataHandle.FilePath, ex);
        }

        return false;
    }
}