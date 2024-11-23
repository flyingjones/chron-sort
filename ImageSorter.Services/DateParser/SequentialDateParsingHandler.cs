using ImageSorter.Services.FileHandling;
using ImageSorter.Services.ProgressLogger;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Services.DateParser;

public class SequentialDateParsingHandler : IDateParsingHandler
{
    private readonly IDateParser _dateParser;
    private readonly ILogger<SequentialDateParsingHandler> _logger;
    private readonly IProgressLogger<SequentialDateParsingHandler> _progressLogger;

    public SequentialDateParsingHandler(
        IDateParser dateParser,
        ILogger<SequentialDateParsingHandler> logger,
        IProgressLogger<SequentialDateParsingHandler> progressLogger)
    {
        _dateParser = dateParser;
        _logger = logger;
        _progressLogger = progressLogger;
    }

    public async Task<IEnumerable<WriteQueueItem>> ScanFiles(string[] filePaths, CancellationToken cancellationToken)
    {
        _progressLogger.LogStart("Reading dates from {numberOfFiles} files (this may take a while)", filePaths.Length);
        
        var result = new WriteQueueItem[filePaths.Length];
        var index = 0;
        foreach (var filePath in filePaths)
        {
            _progressLogger.LogProgress((double) index / filePaths.Length);
            
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
        
        _progressLogger.LogProgressFinished();

        return result;
    }
}