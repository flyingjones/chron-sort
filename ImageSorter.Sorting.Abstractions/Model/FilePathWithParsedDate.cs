namespace ImageSorter.Sorting.Abstractions.Model;

/// <summary>
/// represents the input to the sorter: a FilePath with an associated DateTime
/// </summary>
public record FilePathWithParsedDate
{
    public required string FilePath { get; set; }
    
    /// <summary>
    /// DateTime to sort by
    /// </summary>
    public required DateTime DateTime { get; set; }
}