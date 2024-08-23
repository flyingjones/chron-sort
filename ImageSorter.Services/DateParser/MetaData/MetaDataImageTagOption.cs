using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace ImageSorter.Services.DateParser.MetaData;

public class MetaDataImageTagOption
{
    public required ExifTag<string> Tag { get; init; }
}