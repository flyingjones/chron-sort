using ImageSorter.Services.FileHandling;

namespace ImageSorter.Services.SortConflict;

public interface IConflictDetectionService
{
    /// <summary>
    /// Find conflicts in the <paramref name="writeQueueItems"/>. 
    /// </summary>
    Task<ConflictDetectionResult> FindConflicts(ICollection<WriteQueueItem> writeQueueItems);
}