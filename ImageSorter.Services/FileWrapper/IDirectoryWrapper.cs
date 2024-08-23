namespace ImageSorter.Services.FileWrapper;

/// <summary>
/// Wrapper for <see cref="Directory"/>
/// </summary>
public interface IDirectoryWrapper
{
    /// <inheritdoc cref="Directory.GetFiles(string, string, SearchOption)"/>
    string[] GetFiles(string path, string searchPattern, SearchOption searchOption);

    /// <inheritdoc cref="Directory.CreateDirectory(string)"/>
    DirectoryInfo CreateDirectory(string path);

    /// <inheritdoc cref="Directory.Delete(string)"/>
    void Delete(string path);

    /// <inheritdoc cref="Directory.EnumerateDirectories(string)"/>
    IEnumerable<string> EnumerateDirectories(string path);

    /// <inheritdoc cref="Directory.EnumerateFileSystemEntries(string)"/>
    IEnumerable<string> EnumerateFileSystemEntries(string path);

}