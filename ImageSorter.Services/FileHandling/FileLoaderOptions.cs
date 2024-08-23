namespace ImageSorter.Services.FileHandling;

public class FileLoaderOptions
{
    public required string SourcePath { get; init; }
    
    public string[]? FileEndings { get; init; }
}