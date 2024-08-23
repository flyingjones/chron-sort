namespace ImageSorter.Services.FileHandling;

public class DestinationWriterOptions
{
    public required string DestinationPath { get; init; }
    
    public required string SourcePath { get; init; }
    
    public DateTime? From { get; init; }
    
    public DateTime? To { get; init; }
    
    public required bool OverwriteExistingFiles { get; init; }
}