using ImageSorter.Logging;
using ImageSorter.Services.DateParser;
using ImageSorter.Services.DateParser.MetaData;
using ImageSorter.Services.FileHandling;
using Microsoft.Extensions.DependencyInjection;

namespace ImageSorter.DependencyInjection;

public static class DependencySetupHelper
{
    public static IServiceCollection SetupServices(this RunConfiguration configuration)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddDateParsing(new DateParserConfiguration
        {
            SkipParserAfter = configuration.SkipParserAfter,
            SkipParserBefore = configuration.SkipParserBefore
        });
        serviceCollection.AddStopwatchLogger(configuration.LogLevel);
        serviceCollection.AddSingleton<ILazyFileMetaDataHandleFactory, LazyFileMetaDataHandleFactory>();
        if (configuration.UseDefaultSortConfiguration)
        {
            configuration.SortConfiguration = SortConfigurationFactory.DefaultSorting.ToArray();
        }

        if (configuration.PreferFileNameParsing)
        {
            configuration.SortConfiguration = configuration.SortConfiguration!
                .GroupBy(x => x.Split(':', 2)[0], x => x)
                .OrderBy(x => x.Key == $"{SortType.FileName:G}" ? 0 : 1)
                .SelectMany(x => x)
                .ToArray();
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

        serviceCollection.AddTransient<ISorter, Sorter>();

        return serviceCollection;
    }
}