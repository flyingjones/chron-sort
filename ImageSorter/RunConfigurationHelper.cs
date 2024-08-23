using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;

namespace ImageSorter;

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
        stringBuilder.Append($"Overwrite Existing files: {runConfiguration.Overwrite}{Environment.NewLine}");
        var fileEndingsString = runConfiguration.FilterFileEndings
            ? $"[{string.Join(", ", runConfiguration.FileEndings!)}]"
            : "*";
        stringBuilder.Append($"File Endings            : {fileEndingsString}{Environment.NewLine}");
        var fromDateString = runConfiguration.From?.ToString("o") ?? "-";
        stringBuilder.Append($"From Date               : {fromDateString}{Environment.NewLine}");
        var toDateString = runConfiguration.To?.ToString("o") ?? "-";
        stringBuilder.Append($"To Date                 : {toDateString}{Environment.NewLine}");
        stringBuilder.Append($"Scan in parallel        : {runConfiguration.ScanParallel}{Environment.NewLine}");
        stringBuilder.Append($"OS                      : {RuntimeInformation.OSDescription}");

        return stringBuilder.ToString();
    }
    
    [LoggerMessage(Level = LogLevel.Information, Message = "Starting Image Sorter {runSummary}")]
    private static partial void LogStartupMessage(ILogger logger, string runSummary);
    
    
}