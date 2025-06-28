namespace ImageSorter.Sorting.Abstractions.Model;

public class SortingConflictSummary
{
    public required ICollection<SortedFilePath> NonConflictingFiles { get; set; }

    public required ICollection<SortingConflict> Conflicts { get; set; }
}