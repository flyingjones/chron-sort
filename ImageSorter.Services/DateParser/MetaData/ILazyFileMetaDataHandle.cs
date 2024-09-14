using SixLabors.ImageSharp;
using Image = System.Drawing.Image;

namespace ImageSorter.Services.DateParser.MetaData;

public interface ILazyFileMetaDataHandle : IDisposable
{
    string FilePath { get; }
    
    string FileEnding { get; }

    ImageInfo? GetOrLoadImageInfo();

    Image? GetOrLoadImage();
}