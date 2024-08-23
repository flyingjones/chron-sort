using ImageSorter.Services.FileHandling;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Services.DateParser;

public class SequentialDateParsingHandler : IDateParsingHandler
{
    private readonly IDateParser _dateParser;
    private readonly ILogger<SequentialDateParsingHandler> _logger;

    public SequentialDateParsingHandler(IDateParser dateParser, ILogger<SequentialDateParsingHandler> logger)
    {
        _dateParser = dateParser;
        _logger = logger;
    }

    public async Task<ICollection<WriteQueueItem>> ScanFiles(string[] filePaths, CancellationToken cancellationToken)
    {
        var result = new WriteQueueItem[filePaths.Length];
        var index = 0;
        foreach (var filePath in filePaths)
        {
            try
            {
                var dateTaken = await _dateParser.ParseDate(filePath);
                result[index++] = new WriteQueueItem
                {
                    DateTaken = dateTaken,
                    FilePath = filePath
                };
            }
            catch (Exception exception)
            {
                _logger.LogWarning("Something went wrong while scanning {filePath}: {ex}", filePath, exception);
            }
        }

        return result;
    }
}