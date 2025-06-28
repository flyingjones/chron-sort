using ImageSorter.Sorting.Abstractions.Model;

namespace ImageSorter.Sorting.Abstractions.Services;

public interface IConflictResolver
{
    ICollection<SortedFilePath> ResolveConflicts(
        SortingConflictSummary sortingConflictSummary,
        HashSet<string> filesAtDestination);
}