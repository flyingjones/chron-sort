namespace ImageSorter.Services.FileHandling;

/// <summary>
/// Service to load file paths from disk
/// </summary>
/// <remarks>
/// Is configured by <see cref="FileLoaderOptions"/>
/// </remarks>
public interface IFileLoader
{
    /// <summary>
    /// Returns all file paths which can be found at the <see cref="FileLoaderOptions.SourcePath"/>
    /// </summary>
    /// <remarks>
    /// Will filter by file endings if <see cref="FileLoaderOptions.FileEndings"/> is not null and not empty
    /// </remarks>
    string[] GetFilePaths();
}