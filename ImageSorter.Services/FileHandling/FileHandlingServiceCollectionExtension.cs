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
        serviceCollection.AddSingleton(options);
        serviceCollection.AddSingleton<IFileLoader, FileLoader>();
        return serviceCollection;
    }
}