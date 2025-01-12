namespace ImageSorter.Services.FileHandling;

/// <summary>
/// Helper to create the directory structure year/month as needed
/// </summary>
/// <remarks>
/// Is configured by the <see cref="DestinationWriterOptions"/>
/// <br/>
/// Has an internal state so it knows for which year and month directories exist.
/// </remarks>
public interface IDateDirectory
{
    /// <summary>
    /// Generates a path according to the configured date pattern and creates any missing directories.
    /// Will only call the OS once per directory.
    /// </summary>
    string CreatePathAndDirs(DateTime dateTime);

    /// <summary>
    /// Generates a path according to the configured date pattern. Does not create directories.
    /// </summary>
    string BuildPath(DateTime dateTime);
}