using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;

namespace ImageSorter.DependencyInjection;

public static partial class RunConfigurationHelper
{
    public static void LogRunConfiguration(ILogger logger, RunConfiguration runConfiguration)
    {
        var summary = FormatRunConfiguration(runConfiguration);
        LogStartupMessage(logger, summary);
    }
    
    private static string FormatRunConfiguration(RunConfiguration runConfiguration)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append($"{Environment.NewLine}");
        stringBuilder.Append($"-------------------------");
        stringBuilder.Append($"{Environment.NewLine}");
        stringBuilder.Append($"Source Path             : {runConfiguration.SourcePath}{Environment.NewLine}");
        stringBuilder.Append($"Destination Path        : {runConfiguration.DestinationPath?.FullName ?? "- (running in place)"}{Environment.NewLine}");
        var modeString = runConfiguration.MoveFiles ? "Move" : "Copy";
        var dryRunString = runConfiguration.IsDryRun ? " (dry run)" : string.Empty;
        stringBuilder.Append($"Mode                    : {modeString}{dryRunString}{Environment.NewLine}");
        stringBuilder.Append($"Overwrite Existing files: {runConfiguration.Overwrite}{Environment.NewLine}");
        stringBuilder.Append($"Output format           : {runConfiguration.OutputFormat}{Environment.NewLine}");
        var fileEndingsString = runConfiguration.FilterFileEndings
            ? $"[{string.Join(", ", runConfiguration.FileEndings!)}]"
            : "*";
        stringBuilder.Append($"File Endings            : {fileEndingsString}{Environment.NewLine}");
        var fromDateString = runConfiguration.From?.ToString("o") ?? "-";
        stringBuilder.Append($"From Date               : {fromDateString}{Environment.NewLine}");
        var toDateString = runConfiguration.To?.ToString("o") ?? "-";
        stringBuilder.Append($"To Date                 : {toDateString}{Environment.NewLine}");
        stringBuilder.Append($"Scan in parallel        : {runConfiguration.ScanParallel}{Environment.NewLine}");

        stringBuilder.Append($"Sort Configuration      :{Environment.NewLine}");
        var idx = 0;
        foreach (var sortConfigEntry in runConfiguration.SortConfiguration!)
        {
            stringBuilder.Append($"[   {idx++:00}   ] : {sortConfigEntry}{Environment.NewLine}");
        }
        
        stringBuilder.Append($"[fallback] : <file system update date>{Environment.NewLine}");

        stringBuilder.Append($"Skip parser before      : {runConfiguration.SkipParserBefore:yyyy-MM-dd}{Environment.NewLine}");
        stringBuilder.Append($"Skip parser after       : {runConfiguration.SkipParserAfter:yyyy-MM-dd}{Environment.NewLine}");
        
        stringBuilder.Append($"OS                      : {RuntimeInformation.OSDescription}{Environment.NewLine}");
        stringBuilder.Append($"Version                 : {VersionInformation.Version} (Assembly: {VersionInformation.AssemblyVersion})");

        return stringBuilder.ToString();
    }
    
    [LoggerMessage(Level = LogLevel.Information, Message = "Starting Image Sorter {runSummary}")]
    private static partial void LogStartupMessage(ILogger logger, string runSummary);
    
    
}