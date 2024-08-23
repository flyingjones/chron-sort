using ImageSorter.Services.DateParser.MetaData;

namespace ImageSorter.Services.DateParser;

public class DateParser : IDateParser
{
    private readonly IMetaDataDateParser _metaDataDateParser;
    private readonly IEnumerable<IFileNameDateParser> _fileNameDateParsers;

    public DateParser(IMetaDataDateParser metaDataDateParser, IEnumerable<IFileNameDateParser> fileNameDateParsers)
    {
        _metaDataDateParser = metaDataDateParser;
        _fileNameDateParsers = fileNameDateParsers;
    }

    public async Task<DateTime> ParseDate(string filePath)
    {
        var resultFromMetaData = await _metaDataDateParser.ParseDate(filePath);

        if (resultFromMetaData != null) return resultFromMetaData.Value;

        var parsedPath = new FileInfo(filePath);
        var fileName = parsedPath.Name;
        
        foreach (var fileNameParser in _fileNameDateParsers.OrderBy(x => x.Priority))
        {
            if (fileNameParser.TryParseDateFromFileName(fileName, out var dateTime))
            {
                return dateTime.Value;
            }
        }
        
        return File.GetLastWriteTime(filePath);
    }
}