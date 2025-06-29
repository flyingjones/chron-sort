namespace ImageSorter.Sorting.Abstractions.Model;

public record FilePathWrapper
{
    private readonly string? _filePath;
    private readonly bool _fileSystemCaseSensitive;

    public FilePathWrapper(string filePath, bool fileSystemCaseSensitive)
    {
        _filePath = filePath;
        _fileSystemCaseSensitive = fileSystemCaseSensitive;
    }

    public FilePathWrapper(SortedFilePath sortedFilePath, bool fileSystemCaseSensitive)
    {
        SortedFilePath = sortedFilePath;
        _fileSystemCaseSensitive = fileSystemCaseSensitive;
    }
    
    public SortedFilePath? SortedFilePath { get; }

    /// <summary>
    /// Weather the underlying file comes from the source directory or destination directory
    /// </summary>
    public bool IsFromSource => SortedFilePath != null;

    // one is always not null
    public string SourceFilePath => SortedFilePath?.SourceFilePath ?? _filePath!;

    public string NormalizedSourceFilePath => _fileSystemCaseSensitive ? SourceFilePath : SourceFilePath.ToLower();
    
    // one is always not null
    public string DestinationFilePath => SortedFilePath?.DestinationFilePath ?? _filePath!;
    
    public string NormalizedDestinationPath =>  _fileSystemCaseSensitive ? DestinationFilePath : DestinationFilePath.ToLower();
}