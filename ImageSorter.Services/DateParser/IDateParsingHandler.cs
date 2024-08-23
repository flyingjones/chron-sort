using ImageSorter.Services.FileHandling;

namespace ImageSorter.Services.DateParser;

public interface IDateParsingHandler
{
    Task<IEnumerable<WriteQueueItem>> ScanFiles(string[] filePaths, CancellationToken cancellationToken);
}