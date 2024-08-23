namespace ImageSorter.Services.FileHandling;

public interface IDestinationWriter
{
    /// <summary>
    /// Copy a file to the destination year/month directory
    /// </summary>
    Task CopyFile(string sourcePath, DateTime dateTime, CancellationToken cancellationToken);

    Task CopyFiles(ICollection<WriteQueueItem> writeQueueItems, CancellationToken cancellationToken);
}