namespace ImageSorter.Services.FileHandling;

public class FileOperationResult
{
    public required string SourcePath { get; set; }
    
    public required string DestinationPath { get; set; }
    
    public required FileOperationResultStatus Status { get; set; }
    
    public required DateTime FileDate { get; set; }
    
    public string? ParserName { get; set; }

    public override string ToString()
    {
        return $"[{SourcePath}] -> [{DestinationPath}]: [{Status:G}] [{FileDate:yyyy-MM-dd} {ParserName ?? string.Empty}]";
    }
}