using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;

namespace ImageSorter.Services.DateParser.MetaData;

public class OsAgnosticMetaDataParser : IMetaDataDateParser
{
    private readonly MetaDataDateParserOptions _options;
    private readonly ILogger<WindowsMetaDataDateParser> _logger;

    public OsAgnosticMetaDataParser(MetaDataDateParserOptions options, ILogger<WindowsMetaDataDateParser> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<DateTime?> ParseDate(string filePath)
    {
        if (_options.TagOptions.Length == 0) return null;

        try
        {
            var imageInfo = await Image.IdentifyAsync(filePath);

            foreach (var tagOptions in _options.TagOptions)
            {
                if (imageInfo.Metadata.ExifProfile == null) return null;
                
                if (imageInfo.Metadata.ExifProfile.TryGetValue(tagOptions.Tag, out var dateString))
                {
                    if (dateString.Value == null)
                    {
                        continue;
                    }

                    var result = MetaDataParserHelpers.ParseDateFromTag(dateString.Value);
                    if (result != null) return result;
                }
            }

        }
        catch (Exception ex)
        {
            _logger.LogTrace("error while parsing meta data of file {filePath}: {ex}", filePath, ex);
        }

        return null;
    }
}