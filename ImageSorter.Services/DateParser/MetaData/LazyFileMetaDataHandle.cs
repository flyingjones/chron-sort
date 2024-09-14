using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using Image = System.Drawing.Image;

namespace ImageSorter.Services.DateParser.MetaData;

public partial class LazyFileMetaDataHandle : ILazyFileMetaDataHandle
{
    private readonly ILogger<LazyFileMetaDataHandle> _logger;

    public required string FilePath { get; init; }
    public string FileEnding => FilePath.Split(".")[^1];

    private bool _imageInfoLoadFailed = false;
    private ImageInfo? _imageInfo;

    private bool _imageLoadFailed = false;
    private Image? _image;
    private FileStream? _fileStream;

    public LazyFileMetaDataHandle(ILogger<LazyFileMetaDataHandle> logger)
    {
        _logger = logger;
    }

    public ImageInfo? GetOrLoadImageInfo()
    {
        if (_imageInfoLoadFailed) return null;
        if (_imageInfo != null) return _imageInfo;

        try
        {
            _imageInfo = SixLabors.ImageSharp.Image.Identify(FilePath);
            return _imageInfo;
        }
        catch (Exception ex)
        {
            LogError(ex, FilePath, "SixLabors.ImageSharp.Image.Identify");
            _imageInfoLoadFailed = true;
            return null;
        }
    }

    public Image? GetOrLoadImage()
    {
#pragma warning disable CA1416
        if (_imageLoadFailed) return null;
        if (_image != null) return _image;

        try
        {
            _fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            _image = Image.FromStream(_fileStream, false, false);
            return _image;
        }
        catch (Exception ex)
        {
            LogError(ex, FilePath, "Image.FromStream");
            _imageLoadFailed = true;
            return null;
        }
#pragma warning restore CA1416
    }

    public void Dispose()
    {
        _fileStream?.Dispose();
#pragma warning disable CA1416
        _image?.Dispose();
#pragma warning restore CA1416
        GC.SuppressFinalize(this);
    }

    [LoggerMessage(LogLevel.Debug, "An exception occurred while trying to perform {propertyName} for {filePath}")]
    private partial void LogError(Exception exception, string filePath, string propertyName);
}