using System.Diagnostics.CodeAnalysis;
using ImageSorter.Sorting.Abstractions.Model;

namespace ImageSorter.Sorting.Services.ConflictReducer;

/// <summary>
/// Skips a conflict reduction if it is a trivial overwrite where one file from source and a file from dest are in conflict.
/// If the user wants to overwrite anyway, we have noo need to check for equality
/// </summary>
public class SkipIfTrivialOverwriteDestinationConflictQuickResolver : IDestinationConflictQuickResolver
{
    public bool TryQuickResolve(
        SortingConflict sortingConflict,
        [NotNullWhen(true)] out ReducedSortingConflict? reducedSortingConflict)
    {
        // check that the conflict consists of exactly one file in the source and exactly one file at the destination
        if (sortingConflict.ConflictingFiles.Count == 2 && sortingConflict.ConflictingFiles.Any(x => !x.IsFromSource))
        {
            reducedSortingConflict = new ReducedSortingConflict
            {
                ChosenFiles = sortingConflict.ConflictingFiles
                    .Where(x => x.IsFromSource)
                    .Select(x => x.SortedFilePath!)
                    .ToArray(),
                ConflictingFiles = sortingConflict.ConflictingFiles,
                DiscardedFiles = ArraySegment<SortedFilePath>.Empty
            };
            return true;
        }

        reducedSortingConflict = null;
        return false;
    }
}