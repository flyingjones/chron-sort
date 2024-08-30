using System.Drawing;

namespace ImageSorter.Services.DateParser.MetaData;

public class WindowsFileMetaDataHandleFactory : IFileMetaDataHandleFactory
{
    public Task<FileMetaDataHandle> CreateHandle(string filePath)
    {
        var result = new FileMetaDataHandle
        {
            FilePath = filePath
        };
        try
        {
            result.FileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
#pragma warning disable CA1416
            result.Image = Image.FromStream(result.FileStream, false, false);
#pragma warning restore CA1416
        }
        catch (Exception _)
        {
            result.FileStream?.Dispose();
            result.FileStream = null;
        }

        return Task.FromResult(result);
    }
}