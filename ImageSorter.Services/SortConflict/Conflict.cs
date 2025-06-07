using ImageSorter.Services.FileHandling;

namespace ImageSorter.Services.SortConflict;

public class Conflict
{
    /// <summary>
    /// Files with the same target path which are not equal in content so there is a true conflict
    /// </summary>
    public required ICollection<WriteQueueItem> ConflictingItems { get; set; }
    
    /// <summary>
    /// Files with the same target path which are equal in content to other files in the conflict and which can be safely skipped
    /// </summary>
    public required ICollection<WriteQueueItem> SkippedEqualItems { get; set; }

    /// <summary>
    /// If all files from a conflict have the same content, the conflict resolution is just to use one of the files and ignore the rest
    /// </summary>
    public bool RequiresResolution => ConflictingItems.Count > 1;
}