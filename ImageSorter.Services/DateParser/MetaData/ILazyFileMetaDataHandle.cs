using SixLabors.ImageSharp;
using Image = System.Drawing.Image;

namespace ImageSorter.Services.DateParser.MetaData;

public interface ILazyFileMetaDataHandle : IDisposable
{
    string FilePath { get; }

    ImageInfo? GetOrLoadImageInfo();

    Image? GetOrLoadImage();
}