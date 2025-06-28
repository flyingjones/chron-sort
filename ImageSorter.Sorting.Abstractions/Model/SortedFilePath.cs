using System.Diagnostics.Contracts;

namespace ImageSorter.Sorting.Abstractions.Model;

public record SortedFilePath
{
    public required string SourceFilePath { get; set; }
    
    public required string DestinationFilePath { get; set; }
    
    /// <summary>
    /// DateTime to sort by
    /// </summary>
    public required DateTime DateTime { get; set; }
}