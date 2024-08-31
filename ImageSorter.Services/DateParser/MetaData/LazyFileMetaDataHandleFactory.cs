namespace ImageSorter.Services.DateParser.MetaData;

public class LazyFileMetaDataHandleFactory : ILazyFileMetaDataHandleFactory
{
    public ILazyFileMetaDataHandle CreateHandle(string filePath)
    {
        return new LazyFileMetaDataHandle
        {
            FilePath = filePath
        };
    }
}