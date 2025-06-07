namespace ImageSorter.Services.SortConflict;

public enum ConflictResolutionStrategy
{
    /// <summary>
    /// Do not commit the sorting if a conflict is found
    /// </summary>
    Break,
    
    /// <summary>
    /// Keep all files in case of a conflict
    /// </summary>
    KeepAll,
    
    /// <summary>
    /// Keep one file (with no specification which one)
    /// </summary>
    KeepOne
}