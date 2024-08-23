namespace ImageSorter;

public class RunConfiguration
{
    public required FileInfo SourcePath { get; set; }
    
    public required FileInfo DestinationPath { get; set; }
    
    public string[]? FileEndings { get; set; }

    public bool FilterFileEndings => FileEndings?.Length > 0;
    
    public required bool Overwrite { get; set; }
    
    public DateTime? From { get; set; }
    
    public DateTime? To { get; set; }
}