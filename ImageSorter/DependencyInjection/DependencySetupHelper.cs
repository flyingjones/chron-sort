using ImageSorter.Logging;
using ImageSorter.ProgressLogging;
using ImageSorter.Services.DateParser;
using ImageSorter.Services.DateParser.MetaData;
using ImageSorter.Services.DateTimeWrapper;
using ImageSorter.Services.FileHandling;
using ImageSorter.Services.ProgressLogger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Console;

namespace ImageSorter.DependencyInjection;

public static class DependencySetupHelper
{
    public static IServiceCollection SetupServices(this RunConfiguration configuration)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IDateTimeProvider, DateTimeProvider>();
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
            },
            new DateDirectoryOptions
            {
                DestinationPath = configuration.DestinationPath.FullName,
                Format = configuration.OutputFormat,
                DryRun = configuration.IsDryRun
            },
            configuration.IsDryRun);
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

        if (configuration.UseProgressBar)
        {
            serviceCollection.AddSingleton(new ConsoleProgressLoggerConfiguration());
            serviceCollection.AddTransient(typeof(IProgressLogger<>), typeof(ConsoleProgressLogger<>));
        }
        else
        {
            serviceCollection.AddTransient(typeof(IProgressLogger<>), typeof(NoOperationProgressLogger<>));
        }

        serviceCollection.AddTransient<ISorter, Sorter>();

        return serviceCollection;
    }
}