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
    /// Creates the directory year/month if necessary. Will only call the OS once per directory.
    /// </summary>
    /// <returns>
    /// <see cref="DestinationWriterOptions.DestinationPath"/>/<paramref name="dateTime.Year"/>/<paramref name="dateTime.Month"/>
    /// </returns>
    string CreatePathAndDirs(DateTime dateTime);
}