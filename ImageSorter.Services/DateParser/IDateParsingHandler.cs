using ImageSorter.Services.FileHandling;

namespace ImageSorter.Services.DateParser;

public interface IDateParsingHandler
{
    Task<ICollection<WriteQueueItem>> ScanFiles(string[] filePaths, CancellationToken cancellationToken);
}