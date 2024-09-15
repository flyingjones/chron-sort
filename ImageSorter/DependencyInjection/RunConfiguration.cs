using Microsoft.Extensions.Logging;

namespace ImageSorter.DependencyInjection;

public class RunConfiguration
{
    public required string[]? SortConfiguration { get; set; }
    
    public required bool PreferFileNameParsing { get; set; }

    public bool UseDefaultSortConfiguration => SortConfiguration == null || SortConfiguration.Length == 0;
    
    public required FileInfo SourcePath { get; set; }
    
    public required FileInfo DestinationPath { get; set; }
    
    public required bool MoveFiles { get; set; }
    
    public string[]? FileEndings { get; set; }

    public bool FilterFileEndings => FileEndings?.Length > 0;
    
    public required bool Overwrite { get; set; }
    
    public DateTime? From { get; set; }
    
    public DateTime? To { get; set; }
    
    public required bool ScanParallel { get; set; }
    
    public int? ProgressAt { get; set; }
    
    public LogLevel LogLevel { get; set; }
    
    public required DateTime SkipParserBefore { get; set; }
    
    public required DateTime SkipParserAfter { get; set; }
    
    public required bool IsDryRun { get; set; }
    
    public required string? OutputFormat { get; set; }
}