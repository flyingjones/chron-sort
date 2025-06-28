using ImageSorter.Sorting.Abstractions.Model;

namespace ImageSorter.Sorting.Abstractions.Services;

public interface IConflictFinder
{
    SortingConflictSummary FindConflicts(ICollection<SortedFilePath> filePaths, ICollection<string> filesAtDestination);
}