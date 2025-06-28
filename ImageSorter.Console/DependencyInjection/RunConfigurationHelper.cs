using System.Runtime.InteropServices;
using ImageSorter.Markdown.Abstractions.Model;
using ImageSorter.Markdown.Abstractions.Services;
using ImageSorter.Markdown.Helper;
using Microsoft.Extensions.Logging;

namespace ImageSorter.DependencyInjection;

public static partial class RunConfigurationHelper
{
    public static void LogRunConfiguration(
        ILogger logger,
        IMarkdownTableRenderEngine markdownTableRenderEngine,
        MarkdownTable table)
    {
        LogStartupMessage(logger, $"{Environment.NewLine}{markdownTableRenderEngine.Render(table, false)}");
    }

    public static MarkdownTable FormatRunConfigurationToTable(RunConfiguration runConfiguration)
    {
        var builder = new MarkdownTableBuilder();

        builder.AddRow("Setting Name", "Setting Value");
        
        builder.AddRow("Source Path", runConfiguration.SourcePath.ToString());
        builder.AddRow("Destination Path", runConfiguration.DestinationPath?.FullName ?? "- (running in place)");
        builder.AddRow("Summary Report Path", runConfiguration.SummaryFilePath ?? "-");
        
        var fileEndingsString = runConfiguration.FilterFileEndings
            ? $"[{string.Join(", ", runConfiguration.FileEndings!)}]"
            : "*";
        builder.AddRow("File Endings", fileEndingsString);
        
        var modeString = runConfiguration.MoveFiles ? "Move" : "Copy";
        var dryRunString = runConfiguration.IsDryRun ? " (dry run)" : string.Empty;
        builder.AddRow("Mode", $"{modeString}{dryRunString}");

        builder.AddRow("Conflict Reduction Mode", runConfiguration.ConflictReducerMode.ToString("G"));
        builder.AddRow("Destination Conflict Mode", runConfiguration.DestinationConflictMode.ToString("G"));
        builder.AddRow("Output Format", runConfiguration.OutputFormat);
        
        builder.AddRow("From Date", runConfiguration.From?.ToString("o") ?? "-");
        builder.AddRow("To Date", runConfiguration.To?.ToString("o") ?? "-");
        builder.AddRow("Scan in parallel", runConfiguration.ScanParallel.ToString());

        builder.AddRow("Sort Config");
        
        var sortConfigIdx = 0; 
        foreach (var sortConfigEntry in runConfiguration.SortConfiguration!)
        {
            builder.AddRow($"Sort Config {sortConfigIdx++:00}", sortConfigEntry);
        }
        
        builder.AddRow($"Sort Config {sortConfigIdx:00}", "<file system update date>");
        
        builder.AddRow("End Sort Config");
        
        builder.AddRow("Skip Parser Before", $"{runConfiguration.SkipParserBefore:yyyy-MM-dd}");
        builder.AddRow("Skip Parser After", $"{runConfiguration.SkipParserAfter:yyyy-MM-dd}");
        
        builder.AddRow("System Date", DateTime.Now.ToString("s"));
        builder.AddRow("OS Version", RuntimeInformation.OSDescription);
        builder.AddRow("Sorter Version", $"{VersionInformation.Version} (Assembly: {VersionInformation.AssemblyVersion})");

        return builder.Build();
    }
    
    [LoggerMessage(Level = LogLevel.Information, Message = "Starting Image Sorter with Configuration {runSummary}")]
    private static partial void LogStartupMessage(ILogger logger, string runSummary);
}