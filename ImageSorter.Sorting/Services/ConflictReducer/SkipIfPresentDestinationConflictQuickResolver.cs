using System.Diagnostics.CodeAnalysis;
using ImageSorter.Sorting.Abstractions.Model;

namespace ImageSorter.Sorting.Services.ConflictReducer;

/// <summary>
/// Resolves a conflict by choosing no file if the file(s) are already present at the destination
/// </summary>
public class SkipIfPresentDestinationConflictQuickResolver : IDestinationConflictQuickResolver
{
    public bool TryQuickResolve(
        SortingConflict sortingConflict,
        [NotNullWhen(true)] out ReducedSortingConflict? reducedSortingConflict)
    {
        // check that we have a conflict where a file is already present at the destination
        if (sortingConflict.ConflictingFiles.Any(x => !x.IsFromSource))
        {
            // in this case choose no file (the file is already there!)
            reducedSortingConflict = new ReducedSortingConflict
            {
                ChosenFiles = ArraySegment<SortedFilePath>.Empty,
                DiscardedFiles = sortingConflict.ConflictingFiles
                    .Where(x => x.IsFromSource)
                    .Select(x => x.SortedFilePath!)
                    .ToArray(),
                ConflictingFiles = sortingConflict.ConflictingFiles
            };
            return true;
        }

        reducedSortingConflict = null;
        return false;
    }
}