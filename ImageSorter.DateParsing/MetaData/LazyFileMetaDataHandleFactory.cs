using ImageSorter.DateParsing.Abstractions.Services.MetaData;
using Microsoft.Extensions.Logging;

namespace ImageSorting.DateParsing.MetaData;

public class LazyFileMetaDataHandleFactory : ILazyFileMetaDataHandleFactory
{
    private readonly ILogger<LazyFileMetaDataHandle> _handleLogger;

    // ReSharper disable once ContextualLoggerProblem
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