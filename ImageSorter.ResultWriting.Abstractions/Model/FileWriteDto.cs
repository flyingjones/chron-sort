namespace ImageSorter.ResultWriting.Abstractions.Model;

public record class FileWriteDto
{
    public required string SourcePath { get; set; }
    
    public required string DestinationPath { get; set; }
    
    public required DateTime DateTime { get; set; }
}