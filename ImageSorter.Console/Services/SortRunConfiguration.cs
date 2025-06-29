using ImageSorter.DependencyInjection;
using ImageSorter.Sorting.Model;
using ImageSorter.Sorting.Services;

namespace ImageSorter.Services;

public record SortRunConfiguration
{
    public required string SourcePath { get; init; }
    
    public required string DestinationPath { get; init; }
    
    public required DestinationConflictMode DestinationConflictMode { get; init; }
    
    public required string OriginalDestinationPath { get; init; }
}