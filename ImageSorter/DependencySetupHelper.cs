using ImageSorter.Logging;
using ImageSorter.Services.DateParser;
using ImageSorter.Services.DateParser.MetaData;
using ImageSorter.Services.FileHandling;
using Microsoft.Extensions.DependencyInjection;

namespace ImageSorter;

public static class DependencySetupHelper
{
    public static IServiceCollection SetupServices(this RunConfiguration configuration)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddDateParsing();
        serviceCollection.AddStopwatchLogger();
        serviceCollection.AddFileMetaDataHandleFactory();
        if (configuration.UseDefaultSortConfiguration)
        {
            configuration.SortConfiguration = SortConfigurationFactory.DefaultSorting;
        }
        serviceCollection.ConfigureSorting(configuration.SortConfiguration!);
        
        serviceCollection.AddDestinationWriter(new DestinationWriterOptions
        {
            SourcePath = configuration.SourcePath.FullName,
            DestinationPath = configuration.DestinationPath.FullName,
            OverwriteExistingFiles = configuration.Overwrite,
            From = configuration.From,
            To = configuration.To,
            ProgressCount = configuration.ProgressAt > 0 ? configuration.ProgressAt.Value : int.MaxValue
        });
        serviceCollection.AddFileLoader(new FileLoaderOptions
        {
            SourcePath = configuration.SourcePath.FullName,
            FileEndings = configuration.FileEndings
        });

        if (configuration.ScanParallel)
        {
            serviceCollection.AddSingleton<IDateParsingHandler, ParallelDateParsingHandler>();
        }
        else
        {
            serviceCollection.AddSingleton<IDateParsingHandler, SequentialDateParsingHandler>();
        }

        return serviceCollection;
    }
}