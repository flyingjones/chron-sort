using ImageSorter.Services.DateParser.MetaData;

namespace ImageSorter.Services.DateParser;

public class DateParser : IDateParser
{
    private readonly IFileMetaDataHandleFactory _fileMetaDataHandleFactory;
    private readonly IMetaDataDateParser[] _metaDataDateParsers;
    private readonly IFileNameDateParser[] _fileNameDateParsers;

    public DateParser(IEnumerable<IMetaDataDateParser> metaDataDateParsers,
        IEnumerable<IFileNameDateParser> fileNameDateParsers, IFileMetaDataHandleFactory fileMetaDataHandleFactory)
    {
        _fileMetaDataHandleFactory = fileMetaDataHandleFactory;
        _metaDataDateParsers = metaDataDateParsers.OrderBy(x => x.Priority).ToArray();
        _fileNameDateParsers = fileNameDateParsers.OrderBy(x => x.Priority).ToArray();
    }

    public async Task<DateTime> ParseDate(string filePath)
    {
        var orderedMetaDataParserIndex = 0;
        var orderedFileNameParserIndex = 0;

        var currentMetaDataParser = orderedMetaDataParserIndex < _metaDataDateParsers.Length
            ? _metaDataDateParsers[orderedMetaDataParserIndex]
            : null;

        var currentFileNameParser = orderedFileNameParserIndex < _fileNameDateParsers.Length
            ? _fileNameDateParsers[orderedFileNameParserIndex]
            : null;

        FileMetaDataHandle? metaDataFileHandle = null;
        try
        {
            while (currentMetaDataParser != null || currentFileNameParser != null)
            {
                if (currentMetaDataParser != null &&
                    (currentMetaDataParser.Priority < currentFileNameParser?.Priority || currentFileNameParser == null))
                {
                    if (metaDataFileHandle == null)
                    {
                        metaDataFileHandle = await _fileMetaDataHandleFactory.CreateHandle(filePath);
                    }

                    if (currentMetaDataParser.TryParseDate(metaDataFileHandle, out var result))
                    {
                        return result.Value;
                    }

                    orderedMetaDataParserIndex++;
                    currentMetaDataParser = orderedMetaDataParserIndex < _metaDataDateParsers.Length
                        ? _metaDataDateParsers[orderedMetaDataParserIndex]
                        : null;
                }
                else if (currentFileNameParser != null)
                {
                    if (currentFileNameParser.TryParseDateFromFileName(filePath, out var result))
                    {
                        return result.Value;
                    }

                    orderedFileNameParserIndex++;
                    currentFileNameParser = orderedFileNameParserIndex < _fileNameDateParsers.Length
                        ? _fileNameDateParsers[orderedFileNameParserIndex]
                        : null;
                }
                else
                {
                    // shouldn't happen I think
                    break;
                }
            }
        }
        finally
        {
            metaDataFileHandle?.Dispose();
        }

        return File.GetLastWriteTime(filePath);
    }
}