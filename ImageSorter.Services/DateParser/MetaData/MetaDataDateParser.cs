using System.Drawing;
using System.Text;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Services.DateParser.MetaData;

/// <summary>
/// only works on windows
/// </summary>
public class MetaDataDateParser : IMetaDataDateParser
{
    private readonly MetaDataDateParserOptions _options;
    private readonly ILogger<MetaDataDateParser> _logger;

    public MetaDataDateParser(MetaDataDateParserOptions options, ILogger<MetaDataDateParser> logger)
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
                if (image.PropertyIdList.Contains(tagOptions.TagId))
                {
                    var propertyItem = image.GetPropertyItem(tagOptions.TagId);
                    var propertyValue = Encoding.UTF8.GetString(propertyItem.Value);
                    var result = tagOptions.ParserFund.Invoke(propertyValue);
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