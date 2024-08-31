using Microsoft.Extensions.Logging;

namespace ImageSorter.Services.DateParser.MetaData;

public class LazyFileMetaDataHandleFactory : ILazyFileMetaDataHandleFactory
{
    private readonly ILogger<LazyFileMetaDataHandle> _handleLogger;

    public LazyFileMetaDataHandleFactory(ILogger<LazyFileMetaDataHandle> handleLogger)
    {
        _handleLogger = handleLogger;
    }

    public ILazyFileMetaDataHandle CreateHandle(string filePath)
    {
        return new LazyFileMetaDataHandle(_handleLogger)
        {
            FilePath = filePath
        };
    }
}