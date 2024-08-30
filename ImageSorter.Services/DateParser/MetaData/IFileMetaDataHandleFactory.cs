namespace ImageSorter.Services.DateParser.MetaData;

public interface IFileMetaDataHandleFactory
{
    Task<FileMetaDataHandle> CreateHandle(string filePath);
}