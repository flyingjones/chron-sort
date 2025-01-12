namespace ImageSorter.FileHandling.Directory;

public class ReadOnlyDirectoryWrapper : IDirectoryWrapper
{
    /// <inheritdoc cref="IDirectoryWrapper.GetFiles"/>
    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        return System.IO.Directory.GetFiles(path, searchPattern, searchOption);
    }

    /// <summary>
    /// Doesn't do anything
    /// </summary>
    public DirectoryInfo CreateDirectory(string path)
    {
        return null!;
    }

    /// <summary>
    /// Doesn't do anything
    /// </summary>
    public void Delete(string path)
    {
        // no op
    }

    /// <inheritdoc cref="IDirectoryWrapper.EnumerateDirectories"/>
    public IEnumerable<string> EnumerateDirectories(string path)
    {
        return System.IO.Directory.EnumerateDirectories(path);
    }

    /// <inheritdoc cref="IDirectoryWrapper.EnumerateFileSystemEntries"/>
    public IEnumerable<string> EnumerateFileSystemEntries(string path)
    {
        return System.IO.Directory.EnumerateFileSystemEntries(path);
    }
}