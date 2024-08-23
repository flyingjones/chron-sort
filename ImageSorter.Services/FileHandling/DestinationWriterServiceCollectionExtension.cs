using Microsoft.Extensions.DependencyInjection;

namespace ImageSorter.Services.FileHandling;

public static class DestinationWriterServiceCollectionExtension
{
    public static IServiceCollection AddDestinationWriter(this IServiceCollection serviceCollection, string destinationPath, bool overwrite)
    {
        serviceCollection.AddSingleton(new DestinationWriterOptions
        {
            DestinationPath = destinationPath,
            OverwriteExistingFiles = overwrite
        });
        serviceCollection.AddSingleton<IDestinationWriter, DestinationWriter>();
        return serviceCollection;
    }
}