using System.Collections.Concurrent;
using ImageParser.Utils.ProgressLogger;
using ImageSorter.DateParsing.Abstractions.Model.MetaData;
using ImageSorter.DateParsing.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace ImageSorting.DateParsing;

public class ParallelDateParsingHandler : IDateParsingHandler
{
    private readonly IDateParser _dateParser;
    private readonly ILogger<ParallelDateParsingHandler> _logger;
    private readonly IProgressLogger<ParallelDateParsingHandler> _progressLogger;

    public ParallelDateParsingHandler(
        IDateParser dateParser,
        ILogger<ParallelDateParsingHandler> logger,
        IProgressLogger<ParallelDateParsingHandler> progressLogger)
    {
        _dateParser = dateParser;
        _logger = logger;
        _progressLogger = progressLogger;
    }

    public async Task<IEnumerable<ParsedFileResult>> ScanFiles(string[] filePaths, CancellationToken cancellationToken)
    {
        _progressLogger.LogStart("Reading dates from {numberOfFiles} files (this may take a while)", filePaths.Length);
        
        var writeQueue = new ConcurrentStack<ParsedFileResult>();
        await Parallel.ForEachAsync(filePaths, cancellationToken, async (filePath, _) =>
        {
            _progressLogger.LogProgress((double) writeQueue.Count / filePaths.Length);
            
            if (cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }

            var dateTaken = _dateParser.ParseDate(filePath, out var parserId);
            writeQueue.Push(new ParsedFileResult
            {
                DateTaken = dateTaken,
                FilePath = filePath,
                ParserName = parserId
            });

            await Task.CompletedTask;
        });
        
        _progressLogger.LogProgressFinished();

        return writeQueue;
    }
}