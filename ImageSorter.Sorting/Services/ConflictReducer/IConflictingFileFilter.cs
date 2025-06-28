using System.Diagnostics.CodeAnalysis;
using ImageSorter.Sorting.Abstractions.Model;

namespace ImageSorter.Sorting.Services.ConflictReducer;

public interface IDestinationConflictQuickResolver
{
    bool TryQuickResolve(
        SortingConflict sortingConflict,
        [NotNullWhen(returnValue: true)] out ReducedSortingConflict? reducedSortingConflict);
}