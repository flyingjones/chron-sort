using ImageSorter.FileHandling.Directory;
using ImageSorter.FileHandling.File;
using ImageSorter.FileHandling.FileStream;
using ImageSorter.FileHandling.Path;
using ImageSorter.FileWrapper.Abstractions.Directory;
using ImageSorter.FileWrapper.Abstractions.File;
using ImageSorter.FileWrapper.Abstractions.FileStream;
using ImageSorter.FileWrapper.Abstractions.Path;
using Microsoft.Extensions.DependencyInjection;

namespace ImageSorter.Services.FileHandling;

public static class FileHandlingServiceCollectionExtension
{
    public static IServiceCollection AddDestinationWriter(
        this IServiceCollection serviceCollection,
        DestinationWriterOptions options,
        DateDirectoryOptions dateDirectoryOptions)
    {
        serviceCollection.AddSingleton(dateDirectoryOptions);
        serviceCollection.AddSingleton<IDateDirectory, ConfigurableDateDirectory>();
        serviceCollection.AddSingleton(options);
        serviceCollection.AddSingleton<IDestinationWriter, DestinationWriter>();

        return serviceCollection;
    }

    public static IServiceCollection AddFileLoader(
        this IServiceCollection serviceCollection,
        FileLoaderOptions options,
        bool readOnly)
    {
        if (readOnly)
        {
            serviceCollection.AddSingleton<IDirectoryWrapper, ReadOnlyDirectoryWrapper>();
            serviceCollection.AddSingleton<IFileWrapper, ReadOnlyFileWrapper>();
            serviceCollection.AddSingleton<IFileStreamService, ReadOnlyFileStreamService>();
        }
        else
        {
            serviceCollection.AddSingleton<IDirectoryWrapper, DirectoryWrapper>();
            serviceCollection.AddSingleton<IFileWrapper, ImageSorter.FileHandling.File.FileWrapper>();
            serviceCollection.AddSingleton<IFileStreamService, FileStreamService>();
        }
        
        serviceCollection.AddSingleton<IPathWrapper, PathWrapper>();
        serviceCollection.AddSingleton<IBufferedStreamWriterFactory, BufferedStreamWriterFactory>();
        serviceCollection.AddSingleton(options);
        serviceCollection.AddSingleton<IFileLoader, FileLoader>();
        return serviceCollection;
    }
}