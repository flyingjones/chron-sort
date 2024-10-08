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

    public async Task<IEnumerable<WriteQueueItem>> ScanFiles(string[] filePaths, CancellationToken cancellationToken)
    {
        var result = new WriteQueueItem[filePaths.Length];
        var index = 0;
        foreach (var filePath in filePaths)
        {
            var dateTaken = await Task.Run(() => _dateParser.ParseDate(filePath), cancellationToken);
            result[index++] = new WriteQueueItem
            {
                DateTaken = dateTaken,
                FilePath = filePath
            };

            if (cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }
        }

        return result;
    }
}