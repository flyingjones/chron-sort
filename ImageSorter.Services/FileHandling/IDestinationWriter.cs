namespace ImageSorter.Services.FileHandling;

public interface IDestinationWriter
{
    /// <summary>
    /// Copy a file to the destination year/month directory
    /// </summary>
    Task CopyFile(string sourcePath, DateTime dateTime, CancellationToken cancellationToken);

    /// <summary>
    /// Copy the <paramref name="writeQueueItems"/> to the destination in the directory structure year/month
    /// </summary>
    Task CopyFiles(IEnumerable<WriteQueueItem> writeQueueItems, CancellationToken cancellationToken);

    /// <summary>
    /// Move the <paramref name="writeQueueItems"/> in the source directory to the directory structure year/month
    /// </summary>
    void MoveFiles(IEnumerable<WriteQueueItem> writeQueueItems, CancellationToken cancellationToken);
}