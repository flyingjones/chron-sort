namespace ImageSorter.Sorting.Abstractions.Model;

public record SortingConflict
{
    public required ICollection<FilePathWrapper> ConflictingFiles { get; set; }
}