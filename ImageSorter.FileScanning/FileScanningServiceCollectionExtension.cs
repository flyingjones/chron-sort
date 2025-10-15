using ImageSorter.FileScanning.Abstractions.Services;
using ImageSorter.FileScanning.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ImageSorter.FileScanning;

public static class FileScanningServiceCollectionExtension
{
    public static void AddFileScanning(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IFileScanningService, FileScanningService>();
    }
}