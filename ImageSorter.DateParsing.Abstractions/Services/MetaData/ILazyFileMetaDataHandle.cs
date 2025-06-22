namespace ImageSorter.DateParsing.Abstractions.Services.MetaData;

public interface ILazyFileMetaDataHandle : IDisposable
{
    string FilePath { get; }
    
    string FileEnding { get; }

    IMetaDataTagWrapper GetOrLoadTagWrapper();
}