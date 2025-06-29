using System.Diagnostics.CodeAnalysis;
using ImageSorter.Sorting.Abstractions.Model;
using ImageSorter.Sorting.Model;

namespace ImageSorter.Sorting.Services.FilePath;

public interface IFilePathWrapperFactory
{
    FilePathWrapper Create(string path);

    FilePathWrapper Create(SortedFilePath sortedFilePath);

    [return: NotNullIfNotNull(nameof(path))]
    public string? GetNormalizedPath(string? path);
}