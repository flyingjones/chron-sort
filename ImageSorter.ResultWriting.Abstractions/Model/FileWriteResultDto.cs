namespace ImageSorter.ResultWriting.Abstractions.Model;

public record FileWriteResultDto
{
    public required string SourcePath { get; set; }
    
    public required string DestinationPath { get; set; }

    public required FileOperationResultStatus Status { get; set; }
    
    public required DateTime DateTime { get; set; }
};