using ImageSorter.Services.FileHandling;

namespace ImageSorter.Services.SortConflict;

public class ConflictDetectionResult
{
    public required ICollection<WriteQueueItem> ConflictFreeItems { get; set; }
    
    public required ICollection<Conflict> Conflicts { get; set; }
}