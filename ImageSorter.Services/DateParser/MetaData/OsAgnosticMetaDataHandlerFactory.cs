using SixLabors.ImageSharp;

namespace ImageSorter.Services.DateParser.MetaData;

public class OsAgnosticMetaDataHandlerFactory : IFileMetaDataHandleFactory
{
    public async Task<FileMetaDataHandle> CreateHandle(string filePath)
    {
        try
        {
            return new FileMetaDataHandle
            {
                FilePath = filePath,
                ImageInfo = await Image.IdentifyAsync(filePath)
            };
        }
        catch (Exception _)
        {
            return new FileMetaDataHandle
            {
                FilePath = filePath
            };
        }
    }
}