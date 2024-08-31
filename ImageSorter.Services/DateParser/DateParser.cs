using ImageSorter.Services.DateParser.MetaData;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Services.DateParser;

public partial class DateParser : IDateParser
{
    private readonly IEnumerable<IDateParserImplementation> _dateParserImplementations;
    private readonly ILogger<DateParser> _logger;
    private readonly ILazyFileMetaDataHandleFactory _fileMetaDataHandleFactory;

    public DateParser(IEnumerable<IDateParserImplementation> dateParserImplementations, ILogger<DateParser> logger,
        ILazyFileMetaDataHandleFactory fileMetaDataHandleFactory)
    {
        _dateParserImplementations = dateParserImplementations.OrderBy(x => x.Priority);
        _logger = logger;
        _fileMetaDataHandleFactory = fileMetaDataHandleFactory;
    }


    public DateTime ParseDate(string filePath)
    {
        using var metaDataHandle = _fileMetaDataHandleFactory.CreateHandle(filePath);

        foreach (var dateParserImpl in _dateParserImplementations)
        {
            if (dateParserImpl.TryParseDate(metaDataHandle, out var result))
            {
                LogResult(filePath, dateParserImpl.Name, result.Value);
                return result.Value;
            }
        }
        
        var resultFromFallback = File.GetLastWriteTime(filePath);
        LogResult(filePath, "<file system last write time>", resultFromFallback);
        return resultFromFallback;
    }

    [LoggerMessage(LogLevel.Trace, Message = "Parsing date of file {filePath} using {usedParser}: {result:o}")]
    private partial void LogResult(string filePath, string usedParser, DateTime result);
}