using ImageSorter.DateParsing.Abstractions.Model.MetaData;

namespace ImageSorter.DateParsing.Abstractions.Services;

public interface IDateParsingHandler
{
    Task<IEnumerable<ParsedFileResult>> ScanFiles(string[] filePaths, CancellationToken cancellationToken);
}