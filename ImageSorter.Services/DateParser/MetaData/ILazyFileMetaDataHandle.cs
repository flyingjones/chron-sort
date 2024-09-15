namespace ImageSorter.Services.DateParser.MetaData;

public interface ILazyFileMetaDataHandle : IDisposable
{
    string FilePath { get; }
    
    string FileEnding { get; }

    IMetaDataTagWrapper GetOrLoadTagWrapper();
}