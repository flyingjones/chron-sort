namespace ImageSorter.Services.FileHandling;

public class DestinationWriterOptions
{
    public required string DestinationPath { get; init; }
    
    public required bool OverwriteExistingFiles { get; init; }
}