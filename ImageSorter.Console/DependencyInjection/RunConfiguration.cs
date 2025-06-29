using ImageSorter.Sorting.Model;
using ImageSorter.Sorting.Services;
using Microsoft.Extensions.Logging;

namespace ImageSorter.DependencyInjection;

public class RunConfiguration
{
    public required string[]? SortConfiguration { get; set; }
    
    public required bool PreferFileNameParsing { get; set; }

    public bool UseDefaultSortConfiguration => SortConfiguration == null || SortConfiguration.Length == 0;
    
    public required FileInfo SourcePath { get; set; }
    
    public required FileInfo DestinationPath { get; set; }
    
    public required string? SummaryFilePath { get; set; }
    
    public required FileInfo? SummaryFileDirectoryPath { get; set; }
    
    public required bool EscapeSummaryFileTables { get; set; }
    
    public required bool MoveFiles { get; set; }
    
    public string[]? FileEndings { get; set; }

    public bool FilterFileEndings => FileEndings?.Length > 0;
    
    public DateTime? From { get; set; }
    
    public DateTime? To { get; set; }
    
    public required bool ScanParallel { get; set; }
    
    public int? ProgressAt { get; set; }
    
    public LogLevel LogLevel { get; set; }
    
    public required DateTime SkipParserBefore { get; set; }
    
    public required DateTime SkipParserAfter { get; set; }
    
    public required bool IsDryRun { get; set; }
    
    public required string? OutputFormat { get; set; }
    
    public required bool UseProgressBar { get; set; }
    
    public required string? ProgressBarCharacters { get; set; }
    
    public required ConflictReducerMode ConflictReducerMode { get; set; }
    
    public required DestinationConflictMode DestinationConflictMode { get; set; }
    
    public required ConflictResolverMode ConflictResolverMode { get; set; }
}