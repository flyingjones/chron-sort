using Microsoft.Extensions.Logging;
using Directory = MetadataExtractor.Directory;

namespace ImageSorter.Services.DateParser.MetaData;

public partial class LazyFileMetaDataHandle : ILazyFileMetaDataHandle
{
    private readonly ILogger<LazyFileMetaDataHandle> _logger;

    public required string FilePath { get; init; }
    public string FileEnding => FilePath.Split(".")[^1];

    private IReadOnlyList<Directory>? _directories;
    private bool _directoryLoadFailed = false;

    public LazyFileMetaDataHandle(ILogger<LazyFileMetaDataHandle> logger)
    {
        _logger = logger;
    }

    public IReadOnlyList<Directory>? GetOrLoadMetaDataDirectories()
    {
        if (_directoryLoadFailed) return null;
        if (_directories != null) return _directories;

        try
        {
            _directories = MetadataExtractor.ImageMetadataReader.ReadMetadata(FilePath);
            return _directories;
        }
        catch (Exception ex)
        {
            LogError(ex, FilePath, "MetadataExtractor.ImageMetadataReader.ReadMetadata");
            _directoryLoadFailed = true;
            return null;
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    [LoggerMessage(LogLevel.Debug, "An exception occurred while trying to perform {propertyName} for {filePath}")]
    private partial void LogError(Exception exception, string filePath, string propertyName);
}