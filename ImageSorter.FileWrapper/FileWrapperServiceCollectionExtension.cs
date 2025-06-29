using ImageSorter.FileHandling.CaseSensitivity;
using ImageSorter.FileHandling.Directory;
using ImageSorter.FileHandling.File;
using ImageSorter.FileHandling.FileStream;
using ImageSorter.FileHandling.Path;
using ImageSorter.FileWrapper.Abstractions.Directory;
using ImageSorter.FileWrapper.Abstractions.File;
using ImageSorter.FileWrapper.Abstractions.FileStream;
using ImageSorter.FileWrapper.Abstractions.Path;
using Microsoft.Extensions.DependencyInjection;

namespace ImageSorter.FileHandling;

public static class FileWrapperServiceCollectionExtension
{
    public static IServiceCollection AddFileWrappers(
        this IServiceCollection serviceCollection,
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
        
        return serviceCollection;
    }
}