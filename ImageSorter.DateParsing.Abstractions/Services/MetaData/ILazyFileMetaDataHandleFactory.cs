namespace ImageSorter.DateParsing.Abstractions.Services.MetaData;

public interface ILazyFileMetaDataHandleFactory
{
    ILazyFileMetaDataHandle CreateHandle(string filePath);
}