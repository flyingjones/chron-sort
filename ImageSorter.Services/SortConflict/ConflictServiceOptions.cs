namespace ImageSorter.Services.SortConflict;

public class ConflictServiceOptions
{
    public required ConflictResolutionStrategy ConflictResolutionStrategy { get; set; }
    
    public required RenamingStrategy RenamingStrategy { get; set; }
    
    public required FileEqualsStrategy FileEqualsStrategy { get; set; }
}