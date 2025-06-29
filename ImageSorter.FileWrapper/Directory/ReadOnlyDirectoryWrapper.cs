using ImageSorter.FileHandling.CaseSensitivity;
using ImageSorter.FileWrapper.Abstractions.Directory;

namespace ImageSorter.FileHandling.Directory;

public class ReadOnlyDirectoryWrapper : IDirectoryWrapper
{
    private readonly CaseSensitivityConfiguration _caseSensitivityConfiguration;

    public ReadOnlyDirectoryWrapper(CaseSensitivityConfiguration caseSensitivityConfiguration)
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
        var resultPath = System.IO.Directory.GetParent(path)?.FullName;

        // if the file system is case-insensitive, we cast everything to lower so comparison of paths yields the same
        // equality as the file system
        return _caseSensitivityConfiguration.IsCaseSensitive ? 
            resultPath :
            resultPath?.ToLower();
    }
}