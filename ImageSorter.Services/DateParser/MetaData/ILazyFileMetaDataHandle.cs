using Directory = MetadataExtractor.Directory;

namespace ImageSorter.Services.DateParser.MetaData;

public interface ILazyFileMetaDataHandle : IDisposable
{
    string FilePath { get; }
    
    string FileEnding { get; }

    IReadOnlyList<Directory>? GetOrLoadMetaDataDirectories();
}