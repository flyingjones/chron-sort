using System.Diagnostics.CodeAnalysis;
using ImageSorter.Sorting.Abstractions.Model;

namespace ImageSorter.Sorting.Services.ConflictReducer;

/// <summary>
/// Does nothing
/// </summary>
public class NoOpDestinationConflictQuickResolver : IDestinationConflictQuickResolver
{
    public bool TryQuickResolve(
        SortingConflict sortingConflict,
        [NotNullWhen(true)] out ReducedSortingConflict? reducedSortingConflict)
    {
        reducedSortingConflict = null;
        return false;
    }
}