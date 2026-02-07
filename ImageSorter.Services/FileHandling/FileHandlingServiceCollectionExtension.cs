using Microsoft.Extensions.DependencyInjection;

namespace ImageSorter.Services.FileHandling;

public static class FileHandlingServiceCollectionExtension
{
    public static IServiceCollection AddFileLoader(
        this IServiceCollection serviceCollection,
        FileLoaderOptions options)
    {
        serviceCollection.AddSingleton(options);
        serviceCollection.AddSingleton<IFileLoader, FileLoader>();
        return serviceCollection;
    }
}