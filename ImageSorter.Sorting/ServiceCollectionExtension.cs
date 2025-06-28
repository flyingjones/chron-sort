using ImageSorter.Sorting.Abstractions.Services;
using ImageSorter.Sorting.Model;
using ImageSorter.Sorting.Services;
using ImageSorter.Sorting.Services.ConflictReducer;
using ImageSorter.Sorting.Services.ConflictReducer.FileEqualityMetricImplementation;
using ImageSorter.Sorting.Services.ConflictResolver;
using ImageSorter.Sorting.SubServices.PathBuilder;
using Microsoft.Extensions.DependencyInjection;

namespace ImageSorter.Sorting;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddSorting(
        this IServiceCollection serviceCollection,
        PathBuilderOptions pathBuilderOptions,
        DestinationConflictMode destinationConflictMode,
        ConflictReducerMode conflictReducerMode,
        ConflictResolverMode conflictResolverMode)
    {
        // sorting
        serviceCollection.AddSingleton(pathBuilderOptions);
        serviceCollection.AddSingleton<IPathBuilder, PathBuilder>();
        serviceCollection.AddTransient<ISorter, Sorter>();

        // conflict finder
        switch (destinationConflictMode)
        {
            case DestinationConflictMode.Skip:
                serviceCollection
                    .AddTransient<IDestinationConflictQuickResolver, SkipIfPresentDestinationConflictQuickResolver>();
                break;
            case DestinationConflictMode.Joint:
                serviceCollection
                    .AddTransient<IDestinationConflictQuickResolver, NoOpDestinationConflictQuickResolver>();
                break;
            case DestinationConflictMode.Overwrite:
                serviceCollection
                    .AddTransient<IDestinationConflictQuickResolver, SkipIfTrivialOverwriteDestinationConflictQuickResolver>();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(destinationConflictMode), destinationConflictMode, null);
        }
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

        // TODO add config
        switch (conflictResolverMode)
        {
            case ConflictResolverMode.Throw:
                serviceCollection.AddTransient<IConflictResolver, ThrowingConflictResolver>();
                break;
            case ConflictResolverMode.ChooseOne:
                serviceCollection.AddTransient<IConflictResolver, ChooseArbitraryConflictResolver>();
                break;
            case ConflictResolverMode.SemanticRename:
                serviceCollection.AddTransient<IConflictResolver, SemanticRenameConflictResolver>();
                break;
            case ConflictResolverMode.RandomRename:
                serviceCollection.AddTransient<IConflictResolver, RandomRenameConflictResolver>();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(conflictResolverMode), conflictResolverMode, null);
        }

        serviceCollection.AddTransient<IConflictReducer, ConflictReducer>();

        return serviceCollection;
    }
}