using Microsoft.Extensions.Logging;

namespace ImageSorter.ResultWriting;

public record ResultWriterConfig
{
    public required string SourcePath { get; set; }
    
    public required bool ShouldOverwrite { get; set; }
    
    public required bool ReportSkipAsWarning { get; set; }

    public LogLevel LogLevelForSkips => ReportSkipAsWarning ? LogLevel.Warning : LogLevel.Debug;
}