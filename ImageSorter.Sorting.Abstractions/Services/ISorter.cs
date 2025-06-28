using ImageSorter.Sorting.Abstractions.Model;

namespace ImageSorter.Sorting.Abstractions.Services;

public interface ISorter
{
    ICollection<SortedFilePath> SortFiles(ICollection<FilePathWithParsedDate> filePaths);
}