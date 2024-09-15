namespace ImageSorter.Services.FileHandling;

public class DateDirectoryOptions
{
    public required string? Format { get; set; }
    
    public required string DestinationPath { get; set; }
    
    public required bool DryRun { get; set; }
}