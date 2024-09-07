using ImageSorter.Services.FileWrapper;
using Microsoft.Extensions.DependencyInjection;

namespace ImageSorter.Services.FileHandling;

public static class FileHandlingServiceCollectionExtension
{
    public static IServiceCollection AddDestinationWriter(this IServiceCollection serviceCollection,
        DestinationWriterOptions options, bool dryRun)
    {
        serviceCollection.AddSingleton<IDateDirectory, DateDirectory>();
        serviceCollection.AddSingleton<IFileStreamService, FileStreamService>();
        serviceCollection.AddSingleton(options);
        if (dryRun)
        {
            serviceCollection.AddSingleton<IDestinationWriter, DryRunDestinationWriter>();
        }
        else
        {
            serviceCollection.AddSingleton<IDestinationWriter, DestinationWriter>();
        }

        return serviceCollection;
    }

    public static IServiceCollection AddFileLoader(this IServiceCollection serviceCollection, FileLoaderOptions options)
    {
        serviceCollection.AddSingleton<IDirectoryWrapper, DirectoryWrapper>();
        serviceCollection.AddSingleton<IFileWrapper, FileWrapper.FileWrapper>();
        serviceCollection.AddSingleton<IBufferedStreamWriterFactory, BufferedStreamWriterFactory>();
        serviceCollection.AddSingleton(options);
        serviceCollection.AddSingleton<IFileLoader, FileLoader>();
        return serviceCollection;
    }
}