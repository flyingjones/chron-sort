using ImageSorter.Sorting.Abstractions.Model;
using ImageSorter.Sorting.Abstractions.Services;
using ImageSorter.Sorting.Model;
using ImageSorter.Sorting.Services;
using ImageSorter.Sorting.Services.ConflictReducer;
using ImageSorter.Sorting.Services.ConflictReducer.FileEqualityMetricImplementation;
using ImageSorter.Sorting.Services.ConflictResolver;
using ImageSorter.Sorting.Services.ConflictResolver.PathHashing;
using ImageSorter.Sorting.Services.EquivalenceClassFinder;
using ImageSorter.Sorting.Services.FilePath;
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
        ConflictResolverMode conflictResolverMode,
        bool isFileSystemCaseSensitive)
    {
        // sorting
        serviceCollection.AddTransient<IEquivalenceClassFinder, EquivalenceClassFinder>();
        serviceCollection.AddSingleton(pathBuilderOptions);
        serviceCollection.AddSingleton<IPathBuilder, PathBuilder>();
        serviceCollection.AddTransient<ISorter, Sorter>();

        // conflict finder
        serviceCollection.AddSingleton(new FileSystemConfig
        {
            IsCaseSensitive = isFileSystemCaseSensitive
        });
        serviceCollection.AddTransient<IFilePathWrapperFactory, FilePathWrapperFactory>();
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
                    .AddTransient<IFileEquivalenceMetricImplementation, TrivialIFileContentEquivalenceMetricImplementation>();
                break;
            case ConflictReducerMode.FileLength:
                serviceCollection
                    .AddTransient<IFileEquivalenceMetricImplementation, FileLengthEquivalenceMetricImplementation>();
                break;
            case ConflictReducerMode.FileContent:
                serviceCollection
                    .AddTransient<IFileEquivalenceMetricImplementation, FileContentEquivalenceMetricImplementation>();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(conflictReducerMode), conflictReducerMode, null);
        }

        serviceCollection.AddTransient<IPathHashingService, PathHashingService>();
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
            case ConflictResolverMode.PathHashRename:
                serviceCollection.AddTransient<IConflictResolver, PathHashRenameConflictResolver>();
                break;
            case ConflictResolverMode.ContentHashRename:
                serviceCollection.AddTransient<IConflictResolver, ContentHashRenameConflictResolver>();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(conflictResolverMode), conflictResolverMode, null);
        }

        serviceCollection.AddTransient<IConflictReducer, ConflictReducer>();

        return serviceCollection;
    }
}