namespace ImageSorter.Services.SortConflict;

public enum RenamingStrategy
{
    /// <summary>
    /// Append a numeric index to the file name
    /// </summary>
    /// <example>
    /// <c>Img.jpg</c> may become <c>Img (1).jpg</c>
    /// </example>
    Numeric,
    
    /// <summary>
    /// Adds parts of the file path top the file name until the file names no longer conflict
    /// </summary>
    /// <example>
    /// File <c>Holidays/2024/Spain/Img.jpg</c> and File <c>Holidays/2020/Italy/Img.jpg</c> may become
    /// <c>Spain_Img.jpg</c> and <c>Italy_Img.jpg</c>
    /// </example>
    FlattenPath
}