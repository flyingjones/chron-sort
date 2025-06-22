using ImageSorter.DateParsing.Abstractions.Model.MetaData;

namespace ImageSorter.DateParsing.Abstractions.Services.MetaData;

public interface IMetaDataTagWrapper
{
    string? GetExifTagValue(ExifTagId exifTagId);

    string? GetQuickTimeMovieHeaderValue(QuickTimeMovieHeader header);
    
    bool HasMetaData { get; }
}