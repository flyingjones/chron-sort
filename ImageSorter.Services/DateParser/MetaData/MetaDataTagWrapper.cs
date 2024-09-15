using ImageSorter.Services.DateParser.MetaData.ExifTags;
using ImageSorter.Services.DateParser.MetaData.QuickTimeMovieHeaders;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.QuickTime;
using Directory = MetadataExtractor.Directory;

namespace ImageSorter.Services.DateParser.MetaData;

public class MetaDataTagWrapper : IMetaDataTagWrapper
{
    private readonly IReadOnlyList<Directory>? _directories;

    public MetaDataTagWrapper(IReadOnlyList<Directory>? directories)
    {
        _directories = directories;
    }

    public string? GetExifTagValue(ExifTagId exifTagId)
    {
        return _directories
            ?.Where(x => x is ExifIfd0Directory or ExifSubIfdDirectory)
            .SelectMany(x => x.Tags)
            .FirstOrDefault(tag => tag.Type == (int)exifTagId)
            ?.Description;
    }

    public string? GetQuickTimeMovieHeaderValue(QuickTimeMovieHeader header)
    {
        return _directories
            ?.Where(x => x is QuickTimeMovieHeaderDirectory)
            .SelectMany(x => x.Tags)
            .FirstOrDefault(tag => tag.Type == (int)header)
            ?.Description;
    }

    public bool HasMetaData => _directories != null;
}