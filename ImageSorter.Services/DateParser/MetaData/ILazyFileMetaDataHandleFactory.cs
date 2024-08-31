namespace ImageSorter.Services.DateParser.MetaData;

public interface ILazyFileMetaDataHandleFactory
{
    ILazyFileMetaDataHandle CreateHandle(string filePath);
}