using System.Drawing;
using System.Text;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Services.DateParser.MetaData;

/// <summary>
/// only works on windows -> disable warning that this only works on windows
/// </summary>
#pragma warning disable CA1416
public class WindowsMetaDataDateParser : IMetaDataDateParser
{
    private readonly MetaDataDateParserOptions _options;
    private readonly ILogger<WindowsMetaDataDateParser> _logger;

    public WindowsMetaDataDateParser(MetaDataDateParserOptions options, ILogger<WindowsMetaDataDateParser> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<DateTime?> ParseDate(string filePath)
    {
        if (_options.TagOptions.Length == 0) return null;

        try
        {
            await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            using var image = Image.FromStream(fileStream, false, false);

            foreach (var tagOptions in _options.TagOptions)
            {
                if (image.PropertyIdList.Contains((ushort)tagOptions.Tag))
                {
                    var propertyItem = image.GetPropertyItem((ushort)tagOptions.Tag);
                    var propertyValue = Encoding.UTF8.GetString(propertyItem?.Value ?? Array.Empty<byte>());
                    var result = MetaDataParserHelpers.ParseDateFromTag(propertyValue);
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

#pragma warning restore CA1416