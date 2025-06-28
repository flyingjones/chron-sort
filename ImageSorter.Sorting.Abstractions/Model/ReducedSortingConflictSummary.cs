namespace ImageSorter.Sorting.Abstractions.Model;

public class ReducedSortingConflictSummary
{
    public required ICollection<SortedFilePath> NonConflictingFiles { get; set; }
    
    public required ICollection<ReducedSortingConflict> ReducedSortingConflicts { get; set; }
}