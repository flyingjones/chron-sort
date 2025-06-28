using ImageSorter.Sorting.Abstractions.Model;

namespace ImageSorter.Sorting.Abstractions.Services;

public interface IConflictReducer
{
    /// <summary>
    /// Reduces the amount of conflicts by checking if conflicting files are equal and one can be safely discarded
    /// </summary>
    /// <remarks>
    /// The equality metric can be arbitrarily chosen and is passed into by DI
    /// </remarks>
    ReducedSortingConflict ReduceConflicts(SortingConflict sortingConflict);

    /// <summary>
    /// Reduces the amount of conflicts by checking if conflicting files are equal and one can be safely discarded
    /// </summary>
    /// <remarks>
    /// The equality metric can be arbitrarily chosen and is passed into by DI
    /// </remarks>
    ReducedSortingConflictSummary ReduceConflicts(SortingConflictSummary sortingConflictSummary);
}