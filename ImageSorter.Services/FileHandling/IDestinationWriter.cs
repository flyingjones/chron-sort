namespace ImageSorter.Services.FileHandling;

/// <summary>
/// Service for writing the sorting to disk
/// </summary>
/// <remarks>
/// Is configured by <see cref="DestinationWriterOptions"/>
/// </remarks>
public interface IDestinationWriter
{
    /// <summary>
    /// Copy the <paramref name="writeQueueItems"/> to the destination in the directory structure year/month
    /// </summary>
    Task<ICollection<FileOperationResult>> CopyFiles(ICollection<WriteQueueItem> writeQueueItems, CancellationToken cancellationToken);

    /// <summary>
    /// Move the <paramref name="writeQueueItems"/> in the source directory to the directory structure year/month
    /// </summary>
    ICollection<FileOperationResult> MoveFiles(ICollection<WriteQueueItem> writeQueueItems, CancellationToken cancellationToken);
}