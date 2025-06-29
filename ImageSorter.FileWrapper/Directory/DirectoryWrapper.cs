using ImageSorter.FileHandling.CaseSensitivity;
using ImageSorter.FileWrapper.Abstractions.Directory;

namespace ImageSorter.FileHandling.Directory;

/// <inheritdoc cref="IDirectoryWrapper"/>
public class DirectoryWrapper : IDirectoryWrapper
{
    private readonly CaseSensitivityConfiguration _caseSensitivityConfiguration;

    public DirectoryWrapper(CaseSensitivityConfiguration caseSensitivityConfiguration)
    {
        _caseSensitivityConfiguration = caseSensitivityConfiguration;
    }

    /// <inheritdoc cref="IDirectoryWrapper.GetFiles"/>
    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        // if the file system is case-insensitive, we cast everything to lower so comparison of paths yields the same
        // equality as the file system
        return _caseSensitivityConfiguration.IsCaseSensitive
            ? System.IO.Directory.GetFiles(path, searchPattern, searchOption).ToArray()
            : System.IO.Directory.GetFiles(path, searchPattern, searchOption).Select(x => x.ToLower()).ToArray();
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
        var resultPath = System.IO.Directory.GetParent(path)?.FullName;

        // if the file system is case-insensitive, we cast everything to lower so comparison of paths yields the same
        // equality as the file system
        return _caseSensitivityConfiguration.IsCaseSensitive ? 
            resultPath :
            resultPath?.ToLower();
    }
}