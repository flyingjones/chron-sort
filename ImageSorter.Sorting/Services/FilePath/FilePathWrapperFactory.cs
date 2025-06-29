using System.Diagnostics.CodeAnalysis;
using ImageSorter.Sorting.Abstractions.Model;
using ImageSorter.Sorting.Model;

namespace ImageSorter.Sorting.Services.FilePath;

public class FilePathWrapperFactory : IFilePathWrapperFactory
{
    private readonly bool _isFileSystemCaseSensitive;

    public FilePathWrapperFactory(FileSystemConfig config)
    {
        _isFileSystemCaseSensitive = config.IsCaseSensitive;
    }

    public FilePathWrapper Create(string path)
    {
        return new FilePathWrapper(path, _isFileSystemCaseSensitive);
    }

    public FilePathWrapper Create(SortedFilePath sortedFilePath)
    {
        return new FilePathWrapper(sortedFilePath, _isFileSystemCaseSensitive);
    }

    [return: NotNullIfNotNull(nameof(path))]
    public string? GetNormalizedPath(string? path)
    {
        return _isFileSystemCaseSensitive ? path : path?.ToLower();
    }
}