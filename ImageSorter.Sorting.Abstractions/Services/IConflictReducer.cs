using ImageSorter.Sorting.Abstractions.Model;

namespace ImageSorter.Sorting.Abstractions.Services;

public interface IConflictReducer
{
    /// <summary>
    /// Reduces the amount of conflicts by checking if conflicting files are equivalent and one can be safely discarded
    /// </summary>
    /// <remarks>
    /// The equivalence relation is defined by the different implementations!
    /// </remarks>
    Task<ReducedSortingConflict> ReduceConflicts(SortingConflict sortingConflict, CancellationToken cancellationToken);

    /// <summary>
    /// Reduces the amount of conflicts by checking if conflicting files are equivalent and one can be safely discarded
    /// </summary>
    /// <remarks>
    /// The equivalence relation is defined by the different implementations!
    /// </remarks>
    Task<ReducedSortingConflictSummary> ReduceConflicts(SortingConflictSummary sortingConflictSummary, CancellationToken cancellationToken);
}