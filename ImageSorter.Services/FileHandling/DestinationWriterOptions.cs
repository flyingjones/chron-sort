namespace ImageSorter.Services.FileHandling;

public class DestinationWriterOptions
{
    public required string DestinationPath { get; set; }
    
    public required string SourcePath { get; set; }
    
    public DateTime? From { get; set; }
    
    public DateTime? To { get; set; }
    
    public required bool OverwriteExistingFiles { get; set; }

    public int ProgressCount { get; set; } = 1000;
}