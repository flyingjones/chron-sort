using ImageSorter.Services.DateParser.MetaData.ExifTags;
using ImageSorter.Services.DateParser.MetaData.QuickTimeMovieHeaders;

namespace ImageSorter.Services.DateParser.MetaData;

public interface IMetaDataTagWrapper
{
    string? GetExifTagValue(ExifTagId exifTagId);

    string? GetQuickTimeMovieHeaderValue(QuickTimeMovieHeader header);
    
    bool HasMetaData { get; }
}