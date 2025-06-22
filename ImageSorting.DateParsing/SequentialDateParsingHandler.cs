using ImageParser.Utils.ProgressLogger;
using ImageSorter.DateParsing.Abstractions.Model.MetaData;
using ImageSorter.DateParsing.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace ImageSorting.DateParsing;

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

    public async Task<IEnumerable<ParsedFileResult>> ScanFiles(string[] filePaths, CancellationToken cancellationToken)
    {
        _progressLogger.LogStart("Reading dates from {numberOfFiles} files (this may take a while)", filePaths.Length);
        
        var result = new ParsedFileResult[filePaths.Length];
        var index = 0;
        foreach (var filePath in filePaths)
        {
            _progressLogger.LogProgress((double) index / filePaths.Length);
            
            await Task.Run(() =>
            {
                var resultItem = _dateParser.ParseDate(filePath, out var parserId);
                result[index++] = new ParsedFileResult
                {
                    DateTaken = resultItem,
                    FilePath = filePath,
                    ParserName = parserId
                };
            }, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }
        }
        
        _progressLogger.LogProgressFinished();

        return result;
    }
}