using ImageSorter.FileWrapper.Abstractions.Directory;

namespace ImageSorter.FileHandling.Directory;

public class ReadOnlyDirectoryWrapper : IDirectoryWrapper
{
    /// <inheritdoc cref="IDirectoryWrapper.GetFiles"/>
    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        // to lower to prevent weird issues since on windows the file system is case-insensitive, and we need to
        // have equal paths in case of os says file exists so yay
        return System.IO.Directory.GetFiles(path, searchPattern, searchOption).Select(x => x.ToLower()).ToArray();
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
    
    /// <inheritdoc cref="IDirectoryWrapper.GetParentDirectory"/>
    public string? GetParentDirectory(string path)
    {
        // to lower to prevent weird issues since on windows the file system is case-insensitive, and we need to
        // have equal paths in case of os says file exists so yay
        return System.IO.Directory.GetParent(path)?.FullName.ToLower();
    }
}