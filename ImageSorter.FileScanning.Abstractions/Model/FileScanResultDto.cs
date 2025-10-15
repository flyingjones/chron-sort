namespace ImageSorter.FileScanning.Abstractions.Model;

public class FileScanResultDto
{
    /// <summary>
    /// The file paths with the specified extensions found in the specified directory
    /// </summary>
    public required string[] MatchingFilePaths { get; init; }
    
    /// <summary>
    /// All file paths found in the specified directory
    /// </summary>
    public required string[] AllFilePaths { get; init; }
}