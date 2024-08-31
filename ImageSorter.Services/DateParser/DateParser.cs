using ImageSorter.Services.DateParser.MetaData;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Services.DateParser;

public partial class DateParser : IDateParser
{
    private readonly IEnumerable<IDateParserImplementation> _dateParserImplementations;
    private readonly ILogger<DateParser> _logger;
    private readonly ILazyFileMetaDataHandleFactory _fileMetaDataHandleFactory;
    private readonly DateParserConfiguration _configuration;

    public DateParser(IEnumerable<IDateParserImplementation> dateParserImplementations, ILogger<DateParser> logger,
        ILazyFileMetaDataHandleFactory fileMetaDataHandleFactory, DateParserConfiguration configuration)
    {
        _dateParserImplementations = dateParserImplementations.OrderBy(x => x.Priority);
        _logger = logger;
        _fileMetaDataHandleFactory = fileMetaDataHandleFactory;
        _configuration = configuration;
    }


    public DateTime ParseDate(string filePath)
    {
        using var metaDataHandle = _fileMetaDataHandleFactory.CreateHandle(filePath);

        foreach (var dateParserImpl in _dateParserImplementations)
        {
            if (dateParserImpl.TryParseDate(metaDataHandle, out var result))
            {
                LogResult(filePath, dateParserImpl.Name, result.Value);

                if (result.Value >= _configuration.SkipParserBefore &&
                    result.Value <= _configuration.SkipParserAfter)
                {
                    return result.Value;
                }
                LogSkippedParser(dateParserImpl.Name, filePath, _configuration.SkipParserBefore, result.Value, _configuration.SkipParserAfter);
            }
        }
        
        var resultFromFallback = File.GetLastWriteTime(filePath);
        LogResult(filePath, "<file system last write time>", resultFromFallback);
        return resultFromFallback;
    }

    [LoggerMessage(LogLevel.Debug, Message = "Parser {usedParser} skipped for {filePath} because of suspicious result: {found:yyyy-MM-dd} not element of [{min:yyyy-MM-dd}, {max:yyyy-MM-dd}]")]
    private partial void LogSkippedParser(string usedParser, string filePath, DateTime min, DateTime found, DateTime max);

    [LoggerMessage(LogLevel.Trace, Message = "Parsing date of file {filePath} using {usedParser}: {result:o}")]
    private partial void LogResult(string filePath, string usedParser, DateTime result);
}