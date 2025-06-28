using ImageSorter.FileWrapper.Abstractions.Directory;

namespace ImageSorter.FileHandling.Directory;

/// <inheritdoc cref="IDirectoryWrapper"/>
public class DirectoryWrapper : IDirectoryWrapper
{
    /// <inheritdoc cref="IDirectoryWrapper.GetFiles"/>
    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        return System.IO.Directory.GetFiles(path, searchPattern, searchOption);
    }

    /// <inheritdoc cref="IDirectoryWrapper.CreateDirectory"/>
    public DirectoryInfo CreateDirectory(string path)
    {
        return System.IO.Directory.CreateDirectory(path);
    }

    /// <inheritdoc cref="IDirectoryWrapper.Delete"/>
    public void Delete(string path)
    {
        System.IO.Directory.Delete(path);
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

    /// <inheritdoc cref="IDirectoryWrapper.GetParentDirectory"/>
    public string? GetParentDirectory(string path)
    {
        return System.IO.Directory.GetParent(path)?.FullName;
    }
}