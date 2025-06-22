using System.CommandLine.Invocation;
using ImageSorter.DependencyInjection;
using ImageSorter.FileHandling.FileStream;
using ImageSorter.Markdown.Abstractions.Model;
using ImageSorter.Markdown.Abstractions.Services;
using ImageSorter.Markdown.Services;
using ImageSorter.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImageSorter;

public static class RootCommandHandler
{
    /// <summary>
    /// Entrypoint into the program
    /// </summary>
    public static async Task Handle(InvocationContext context)
    {
        // parse arguments and build run config
        var parsedContext = context.ParseResult;
        var runConfiguration = RootCommandFactory.ParseRunConfiguration(parsedContext);

        // TODO add conflict resolution:
        // - error: (Default) stop sorting if conflicts occur
        // - keep: rename files with (x) to resolve conflicts
        // - skip: chose one file and skip the rest
        // - auto: make a comparison of the conflicting files. if they have the same content, skip one, otherwise keep both
        // add conflict resolution summary
        
        // also maybe add a new tool to flatten directory names into the file names
        // e.g. ImagesHoliday/img01.jpg -> ImagesHoliday_img01.jpg
        
        // create the summary file writer
        await using var markdownFileWriter = CreateMarkdownFileWriter(
            runConfiguration.SummaryFileDirectoryPath?.FullName,
            runConfiguration.SummaryFilePath,
            runConfiguration.EscapeSummaryFileTables);
        
        // set up the service provider
        var serviceProvider = runConfiguration
            .SetupServices()
            .AddSingleton<IMarkdownFileWriter>(markdownFileWriter)
            .BuildServiceProvider();
            
        // log the configuration
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        var markDownRenderEngine = serviceProvider.GetRequiredService<IMarkdownTableRenderEngine>();
        var runConfigurationTable = RunConfigurationHelper.FormatRunConfigurationToTable(runConfiguration);
        RunConfigurationHelper.LogRunConfiguration(logger, markDownRenderEngine, runConfigurationTable);
        WriteToSummaryFile(markdownFileWriter, runConfigurationTable);

        // get the sorter and perform the sorting
        var sorter = serviceProvider.GetRequiredService<ISorter>();
        await sorter.PerformSorting(runConfiguration.MoveFiles, context.GetCancellationToken());
    }

    private static void WriteToSummaryFile(IMarkdownFileWriter writer, MarkdownTable runConfigurationTable)
    {
        writer.WriteHeading(MarkdownHeading.H1, "Sorting Summary");
        writer.WriteHeading(MarkdownHeading.H2, "Configuration");
        writer.WriteTable(runConfigurationTable);
        writer.WriteLine();
    }

    private static IMarkdownFileWriter CreateMarkdownFileWriter(
        string? directory,
        string? path,
        bool escapeTables)
    {
        if (!string.IsNullOrWhiteSpace(directory) && !string.IsNullOrWhiteSpace(path))
        {
            var streamFactory = new BufferedStreamWriterFactory();

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return new MarkdownFileWriter(
                streamFactory.CreateStreamWriter(path, FileMode.CreateNew),
                new MarkdownTableRenderEngine(),
                escapeTables);
        }

        return new MockMarkdownFileWriter();
    }
}