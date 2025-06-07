namespace ImageSorter.Services.SortConflict;

public enum FileEqualsStrategy
{
    /// <summary>
    /// All files with the same name are considered to have different content
    /// </summary>
    None,
    
    /// <summary>
    /// Files with the same name are considered to be equal if they have the same length
    /// </summary>
    FileLength,
    
    /// <summary>
    /// Files with the same name are considered to be equal if they are equal byte for byte
    /// </summary>
    FileContent
}