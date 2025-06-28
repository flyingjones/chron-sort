using System.Diagnostics.CodeAnalysis;

namespace ImageSorter.Sorting.Abstractions.Model;

public record FilePathWrapper
{
    private readonly string? _filePath;

    public FilePathWrapper(string filePath)
    {
        _filePath = filePath;
    }

    public FilePathWrapper(SortedFilePath sortedFilePath)
    {
        SortedFilePath = sortedFilePath;
    }
    
    public SortedFilePath? SortedFilePath { get; }

    /// <summary>
    /// Weather the underlying file comes from the source directory or destination directory
    /// </summary>
    public bool IsFromSource => SortedFilePath != null;

    // one is always not null
    public string SourceFilePath => SortedFilePath?.SourceFilePath ?? _filePath!;
    
    // one is always not null
    public string DestinationFilePath => SortedFilePath?.DestinationFilePath ?? _filePath!;
}