using System.Collections.Concurrent;
using ImageSorter.Services.FileHandling;
using ImageSorter.Services.ProgressLogger;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Services.DateParser;

public class ParallelDateParsingHandler : IDateParsingHandler
{
    private readonly IDateParser _dateParser;
    private readonly ILogger<ParallelDateParsingHandler> _logger;
    private readonly IProgressLogger<ParallelDateParsingHandler> _progressLogger;

    public ParallelDateParsingHandler(IDateParser dateParser, ILogger<ParallelDateParsingHandler> logger,
        IProgressLogger<ParallelDateParsingHandler> progressLogger)
    {
        _dateParser = dateParser;
        _logger = logger;
        _progressLogger = progressLogger;
    }

    public async Task<IEnumerable<WriteQueueItem>> ScanFiles(string[] filePaths, CancellationToken cancellationToken)
    {
        _progressLogger.LogStart("Reading dates from {numberOfFiles} files (this may take a while)", filePaths.Length);
        
        var writeQueue = new ConcurrentStack<WriteQueueItem>();
        await Parallel.ForEachAsync(filePaths, cancellationToken, async (filePath, _) =>
        {
            _progressLogger.LogProgress((double) writeQueue.Count / filePaths.Length);
            
            if (cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }

            var dateTaken = _dateParser.ParseDate(filePath);
            writeQueue.Push(new WriteQueueItem
            {
                DateTaken = dateTaken,
                FilePath = filePath
            });

            await Task.CompletedTask;
        });
        
        _progressLogger.LogProgressFinished();

        return writeQueue;
    }
}