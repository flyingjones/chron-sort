using ImageSorter.Sorting.Abstractions.Services;
using ImageSorter.Sorting.Model;
using ImageSorter.Sorting.Services;
using ImageSorter.Sorting.Services.ConflictReducer;
using ImageSorter.Sorting.Services.ConflictReducer.FileEqualityMetricImplementation;
using ImageSorter.Sorting.SubServices.PathBuilder;
using Microsoft.Extensions.DependencyInjection;

namespace ImageSorter.Sorting;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddSorting(
        this IServiceCollection serviceCollection,
        PathBuilderOptions pathBuilderOptions,
        ConflictReducerMode conflictReducerMode)
    {
        // sorting
        serviceCollection.AddSingleton(pathBuilderOptions);
        serviceCollection.AddSingleton<IPathBuilder, PathBuilder>();
        serviceCollection.AddTransient<ISorter, Sorter>();

        // conflict finder
        serviceCollection.AddTransient<IConflictFinder, ConflictFinder>();
        
        // conflict reducer
        switch (conflictReducerMode)
        {
            case ConflictReducerMode.None:
                serviceCollection
                    .AddTransient<IFileEqualityMetricImplementation, TrivialFileContentEqualityMetricImplementation>();
                break;
            case ConflictReducerMode.FileLength:
                serviceCollection
                    .AddTransient<IFileEqualityMetricImplementation, FileLengthEqualityMetricImplementation>();
                break;
            case ConflictReducerMode.FileContent:
                serviceCollection
                    .AddTransient<IFileEqualityMetricImplementation, FileContentEqualityMetricImplementation>();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(conflictReducerMode), conflictReducerMode, null);
        }

        serviceCollection.AddTransient<IConflictReducer, ConflictReducer>();

        return serviceCollection;
    }
}