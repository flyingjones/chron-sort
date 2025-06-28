using ImageSorter.Sorting.Abstractions.Exceptions;
using ImageSorter.Sorting.Abstractions.Model;
using ImageSorter.Sorting.Abstractions.Services;

namespace ImageSorter.Sorting.Services.ConflictResolver;

/// <summary>
/// Throws an exception if a conflict exists
/// </summary>
public class ThrowingConflictResolver : IConflictResolver
{
    public ICollection<SortedFilePath> ResolveConflicts(SortingConflictSummary sortingConflictSummary, HashSet<string> filesAtDestination)
    {
        if (sortingConflictSummary.Conflicts.Count > 0)
        {
            throw new ConflictException("Sorting conflict detected but resolving is disabled!");
        }

        return sortingConflictSummary.NonConflictingFiles;
    }
}