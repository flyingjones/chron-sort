using System.Diagnostics.Contracts;

namespace ImageSorter.Sorting.Abstractions.Model;

/// <summary>
/// Represents a sorting conflict where files are checked if they are equal (according to a metric). for equal files,
/// one is chosen and the others are discarded
/// </summary>
public record ReducedSortingConflict
{
    /// <summary>
    /// All files in this conflict
    /// </summary>
    public required ICollection<FilePathWrapper> ConflictingFiles { get; set; }

    /// <summary>
    /// Files to keep (each file here is distinct from each other)
    /// </summary>
    /// <remarks>
    /// May be empty if file is already present at destination
    /// </remarks>
    public required ICollection<SortedFilePath> ChosenFiles { get; set; }

    /// <summary>
    /// Files to ignore (for each file one other equal file is in <see cref="ChosenFiles"/>
    /// </summary>
    public required ICollection<SortedFilePath> DiscardedFiles { get; set; }

    public bool IsResolved =>
        // all files are present at the destination
        ChosenFiles.Count == 0 ||
        // file is not present at destination but all files at source are equal
        (ChosenFiles.Count == 1 && ConflictingFiles.All(x => x.IsFromSource));
    
    /// <summary>
    /// Conflict is resolved when existing files shall be overwritten
    /// </summary>
    public bool IsResolvedInSource => ChosenFiles.Count <= 1;

    public DateTime DateTime => ConflictingFiles.First(x => x.IsFromSource).SortedFilePath!.DateTime;
}