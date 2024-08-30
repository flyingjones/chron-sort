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
        ILogger<IMetaDataDateParser> logger)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // MetaDataDateParser uses windows specific apis
            serviceCollection.AddSingleton<IMetaDataDateParser>(new WindowsMetaDataDateParser(exifTagId, priority, logger));
        }
        else
        {
            serviceCollection.AddSingleton<IMetaDataDateParser>(new OsAgnosticMetaDataDateParser(exifTagId, priority, logger));
        }
        
        return serviceCollection;
    }
    
    public static IServiceCollection AddFileMetaDataHandleFactory(this IServiceCollection serviceCollection)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // MetaDataDateParser uses windows specific apis
            serviceCollection.AddScoped<IFileMetaDataHandleFactory, WindowsFileMetaDataHandleFactory>();
        }
        else
        {
            serviceCollection.AddScoped<IFileMetaDataHandleFactory, OsAgnosticMetaDataHandlerFactory>();
        }

        return serviceCollection;
    }
}