using System.Collections.Concurrent;
using ImageSorter.Services.FileHandling;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Services.DateParser;

public class ParallelDateParsingHandler : IDateParsingHandler
{
    private readonly IDateParser _dateParser;
    private readonly ILogger<ParallelDateParsingHandler> _logger;

    public ParallelDateParsingHandler(IDateParser dateParser, ILogger<ParallelDateParsingHandler> logger)
    {
        _dateParser = dateParser;
        _logger = logger;
    }

    public async Task<ICollection<WriteQueueItem>> ScanFiles(string[] filePaths, CancellationToken cancellationToken)
    {
        var writeQueue = new ConcurrentStack<WriteQueueItem>();
        await Parallel.ForEachAsync(filePaths, cancellationToken, async (filePath, _) =>
        {
            try
            {
                var dateTaken = await _dateParser.ParseDate(filePath);
                writeQueue.Push(new WriteQueueItem
                {
                    DateTaken = dateTaken,
                    FilePath = filePath
                });
            }
            catch (Exception exception)
            {
                _logger.LogWarning("Something went wrong while scanning {filePath}: {ex}", filePath, exception);
            }
        });

        return writeQueue.ToList();
    }
}