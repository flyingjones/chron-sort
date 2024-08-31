using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Services.DateParser.MetaData;

public static class MetaDataParserServiceCollectionExtension
{
    public static IServiceCollection AddMetaDataParser(
        this IServiceCollection serviceCollection,
        ExifTagId exifTagId,
        int priority,
        ILogger<IDateParserImplementation> logger)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // MetaDataDateParser uses windows specific apis
            serviceCollection.AddSingleton<IDateParserImplementation>(new WindowsMetaDataDateParser(exifTagId, priority, logger));
        }
        else
        {
            serviceCollection.AddSingleton<IDateParserImplementation>(new OsAgnosticMetaDataDateParser(exifTagId, priority, logger));
        }
        
        return serviceCollection;
    }
    
    public static IServiceCollection AddFileMetaDataHandleFactory(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ILazyFileMetaDataHandleFactory, LazyFileMetaDataHandleFactory>();

        return serviceCollection;
    }
}