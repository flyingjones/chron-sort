using ImageParser.Utils.DateTimeWrapper;
using ImageParser.Utils.ProgressLogger;
using ImageParser.Utils.RandomWrapper;
using ImageSorter.DateParsing.Abstractions.Services;
using ImageSorter.DateParsing.Abstractions.Services.MetaData;
using ImageSorter.FileHandling;
using ImageSorter.FileScanning;
using ImageSorter.Logging;
using ImageSorter.Markdown.Abstractions.Services;
using ImageSorter.Markdown.Services;
using ImageSorter.ProgressLogging;
using ImageSorter.ResultWriting;
using ImageSorter.ResultWriting.Abstractions;
using ImageSorter.Services;
using ImageSorter.Sorting;
using ImageSorter.Sorting.Model;
using ImageSorter.Sorting.SubServices.PathBuilder;
using ImageSorting.DateParsing;
using ImageSorting.DateParsing.MetaData;
using Microsoft.Extensions.DependencyInjection;

namespace ImageSorter.DependencyInjection;

public static class DependencySetupHelper
{
    public static IServiceCollection SetupServices(this RunConfiguration configuration)
    {
        PreprocessRunConfiguration(configuration);
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IDateTimeProvider, DateTimeProvider>();
        serviceCollection.AddTransient<IRandomStringGenerator>(_ => RandomStringGenerator.CreateForLowerCaseLetters());
        serviceCollection.AddTransient<IMarkdownTableRenderEngine, MarkdownTableRenderEngine>();

        serviceCollection.AddFileWrappers(configuration.IsDryRun);
        serviceCollection.AddFileScanning();
        
        serviceCollection.AddDateParsing(new DateParserConfiguration
        {
            SkipParserAfter = configuration.SkipParserAfter,
            SkipParserBefore = configuration.SkipParserBefore
        });
        serviceCollection.AddStopwatchLogger(configuration.LogLevel);
        serviceCollection.AddSingleton<ILazyFileMetaDataHandleFactory, LazyFileMetaDataHandleFactory>();

        serviceCollection.ConfigureSorting(configuration.SortConfiguration!);

        serviceCollection.AddSingleton(
            new ResultWriterConfig
            {
                ShouldOverwrite = configuration.DestinationConflictMode == DestinationConflictMode.Overwrite,
                // skips should only happen if skip is selected, otherwise all conflicts should be resolved earlier!
                ReportSkipAsWarning = configuration.DestinationConflictMode != DestinationConflictMode.Skip,
                SourcePath = configuration.SourcePath.FullName
            });
        if (configuration.MoveFiles)
        {
            serviceCollection.AddTransient<IResultWriter, MoveFileResultWriter>();
        }
        else
        {
            serviceCollection.AddTransient<IResultWriter, CopyFileResultWriter>();
        }

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
            var progressLoggerConfig = new ConsoleProgressLoggerConfiguration();

            if (configuration.ProgressBarCharacters != null)
            {
                progressLoggerConfig.ProgressCharsString = configuration.ProgressBarCharacters;
            }

            serviceCollection.AddSingleton(progressLoggerConfig);
            serviceCollection.AddTransient(typeof(IProgressLogger<>), typeof(ConsoleProgressLogger<>));
        }
        else
        {
            serviceCollection.AddTransient(typeof(IProgressLogger<>), typeof(NoOperationProgressLogger<>));
        }

        var casingAwareDestPath = configuration.FileSystemIsCaseSensitive
            ? configuration.DestinationPath.FullName
            : configuration.DestinationPath.FullName.ToLower();
        serviceCollection.AddSingleton(new SortRunConfiguration
        {
            DestinationConflictMode = configuration.DestinationConflictMode,
            SourcePath = configuration.SourcePath.FullName,
            DestinationPath = casingAwareDestPath,
            OriginalDestinationPath = configuration.DestinationPath.FullName,
            FileExtensions = configuration.FileEndings
        });
        serviceCollection.AddTransient<ISorter, Sorter>();
        serviceCollection.AddTransient<ISummarizeService, SummarizeService>();
        serviceCollection.AddTransient<ISummaryReportingService, SummaryReportingService>();

        serviceCollection.AddSorting(
            new PathBuilderOptions
            {
                DestinationPath = casingAwareDestPath,
                Format = configuration.OutputFormat
            },
            configuration.DestinationConflictMode,
            configuration.ConflictReducerMode,
            configuration.ConflictResolverMode,
            configuration.FileSystemIsCaseSensitive);

        return serviceCollection;
    }

    private static void PreprocessRunConfiguration(RunConfiguration configuration)
    {
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
    }
}