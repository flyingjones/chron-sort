using ImageSorter.Sorting.Abstractions.Model;

namespace ImageSorter.Sorting.Abstractions.Services;

public interface IConflictResolver
{
    ICollection<SortedFilePath> ResolveConflicts(
        SortingConflictSummary sortingConflictSummary,
        string[] filesAtDestination,
        out ICollection<SortedFilePath> discardedFiles);
}