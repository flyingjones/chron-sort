using System.Diagnostics.CodeAnalysis;
using ImageSorter.Sorting.Abstractions.Model;

namespace ImageSorter.Sorting.Services.ConflictReducer;

/// <summary>
/// This is intended to reduce / resolve conflicts between the source files and files already present in the destination
/// without accessing any additional IO. This is useful for implementing conflict resolution which always overwrites or
/// always skips files present at the destination
/// </summary>
public interface IDestinationConflictQuickResolver
{
    /// <summary>
    /// Tries to resolve / reduce a conflict.
    /// Returns true if a conflict could be reduced / resolved. In this case the reduced / resolved conflict is returned
    /// as the out parameter
    /// </summary>
    bool TryQuickResolve(
        SortingConflict sortingConflict,
        [NotNullWhen(returnValue: true)] out ReducedSortingConflict? reducedSortingConflict);
}