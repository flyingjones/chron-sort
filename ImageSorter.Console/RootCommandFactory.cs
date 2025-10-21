using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using ImageSorter.DateParsing.Abstractions.Model.MetaData;
using ImageSorter.DependencyInjection;
using ImageSorter.FileHandling.CaseSensitivity;
using ImageSorter.Services;
using ImageSorter.Sorting.Model;
using Microsoft.Extensions.Logging;

namespace ImageSorter;

public static class RootCommandFactory
{
    private static readonly string Description = $"""
                                                  Sorts files chronologically in the specified directory structure (default: year/month)
                                                  Default sort configuration (change with --configure):
                                                  {string.Join(Environment.NewLine, SortConfigurationFactory.DefaultSorting)}
                                                  """;

    public static RootCommand BuildRootCommand()
    {
        var rootCommand = new RootCommand(description: Description);

        // args
        rootCommand.Arguments.Add(Arguments.SourcePathArgument);

        // options
        // main settings
        rootCommand.Options.Add(Options.DestinationPathOption);
        rootCommand.Options.Add(Options.MoveOption);
        rootCommand.Options.Add(Options.IsDryRunOption);
        rootCommand.Options.Add(Options.FormatOption);
        // parser config
        rootCommand.Options.Add(Options.SortConfigurationOption);
        rootCommand.Options.Add(Options.SkipParserWhenDateBeforeOption);
        rootCommand.Options.Add(Options.SkipParserWhenDateAfterOption);
        // file filter
        rootCommand.Options.Add(Options.FileEndingFilterOption);
        rootCommand.Options.Add(Options.UseFromDateFilterOption);
        rootCommand.Options.Add(Options.UseToDateFilterOption);
        // optimizations
        rootCommand.Options.Add(Options.PreferFileNameParsingOption);
        rootCommand.Options.Add(Options.UseParallelScanningOption);
        // conflict reduction
        rootCommand.Options.Add(Options.ConflictReducerModeOption);
        rootCommand.Options.Add(Options.DestinationConflictModeOption);
        // conflict resolution
        rootCommand.Options.Add(Options.ConflictResolverModeOption);
        // logging
        rootCommand.Options.Add(Options.LogLevelOption);
        rootCommand.Options.Add(Options.BeVerboseOption);
        rootCommand.Options.Add(Options.UseProgressBar);
        rootCommand.Options.Add(Options.ProgressBarString);
        rootCommand.Options.Add(Options.SummaryFilePathOption);
        rootCommand.Options.Add(Options.EscapeSummaryMarkdownTables);

        return rootCommand;
    }

    public static RunConfiguration ParseRunConfiguration(ParseResult parsedContext)
    {
        var destPath = parsedContext.GetValue(Options.DestinationPathOption);
        var isMoveFiles = parsedContext.GetValue(Options.MoveOption);

        if (destPath == null && !isMoveFiles)
        {
            throw new ArgumentException("no destination path provided but in-place was not set");
        }

        var runConfig = new RunConfiguration
        {
            SourcePath = parsedContext.GetRequiredValue(Arguments.SourcePathArgument).FullName,
            SortConfiguration = parsedContext.GetValue(Options.SortConfigurationOption),
            PreferFileNameParsing = parsedContext.GetValue(Options.PreferFileNameParsingOption),
            DestinationPath = destPath?.FullName ?? parsedContext.GetRequiredValue(Arguments.SourcePathArgument).FullName,
            MoveFiles = isMoveFiles,
            FileEndings = parsedContext.GetValue(Options.FileEndingFilterOption),
            // TODO fix from filter
            From = parsedContext.GetValue(Options.UseFromDateFilterOption),
            // TODO fix to filter
            To = parsedContext.GetValue(Options.UseToDateFilterOption),
            ScanParallel = parsedContext.GetValue(Options.UseParallelScanningOption),
            LogLevel = parsedContext.GetValue(Options.BeVerboseOption)
                ? LogLevel.Trace
                : parsedContext.GetValue(Options.LogLevelOption),
            SkipParserBefore = parsedContext.GetValue(Options.SkipParserWhenDateBeforeOption),
            SkipParserAfter = parsedContext.GetValue(Options.SkipParserWhenDateAfterOption),
            IsDryRun = parsedContext.GetValue(Options.IsDryRunOption),
            OutputFormat = parsedContext.GetValue(Options.FormatOption),
            UseProgressBar = parsedContext.GetValue(Options.UseProgressBar),
            ProgressBarCharacters = parsedContext.GetValue(Options.ProgressBarString),
            SummaryFileDirectoryPath = parsedContext.GetValue(Options.SummaryFilePathOption),
            SummaryFilePath = GenerateSummaryFilePath(parsedContext.GetValue(Options.SummaryFilePathOption)?.FullName),
            EscapeSummaryFileTables = parsedContext.GetValue(Options.EscapeSummaryMarkdownTables),
            ConflictReducerMode = parsedContext.GetValue(Options.ConflictReducerModeOption) ?? ConflictReducerMode.None,
            DestinationConflictMode = parsedContext.GetValue(Options.DestinationConflictModeOption) ??
                                      DestinationConflictMode.Joint,
            ConflictResolverMode =
                parsedContext.GetValue(Options.ConflictResolverModeOption) ?? ConflictResolverMode.Throw,
            CaseSensitivityDetectionMode = parsedContext.GetValue(Options.CaseSensitivityDetectionOption) ??
                                           CaseSensitivityDetectionMode.Auto
        };
        return runConfig;
    }

    [return: NotNullIfNotNull(nameof(basePath))]
    private static string? GenerateSummaryFilePath(string? basePath)
    {
        if (basePath == null) return null;

        var fileName = $"sort_summary_{DateTime.Now:yyyy-MM-dd_hh_mm_ss}.md";

        return Path.GetFullPath(Path.Combine(basePath, fileName));
    }

    private static class Arguments
    {
        public static readonly Argument<FileInfo> SourcePathArgument = new("source path")
        {
            Description = "The path of the source directory"
        };
    }

    private static class Options
    {
        public static readonly Option<FileInfo?> DestinationPathOption = new("--dest", "--out")
        {
            Description = "The path of the destination directory (required if not --move)"
        };

        public static readonly Option<bool> MoveOption = new("--move")
        {
            Description = "Move files instead of copy",
            DefaultValueFactory = _ => false
        };

        public static readonly Option<string[]> FileEndingFilterOption = new("--types", "-t")
        {
            Description = "Space seperated list of file endings to sort"
        };

        public static readonly Option<bool> UseParallelScanningOption = new("--scan-parallel")
        {
            Description = "Perform the scan part in parallel",
            DefaultValueFactory = _ => false
        };

        public static readonly Option<DateTime?> UseFromDateFilterOption = new("--from")
        {
            Description = "Minimum date for files to sort"
        };

        public static readonly Option<DateTime?> UseToDateFilterOption = new("--to")
        {
            Description = "Maximum date for files to sort"
        };

        public static readonly Option<string[]> SortConfigurationOption = new("-c", "--configure")
        {
            Description = $"""
                           Custom sort configuration. Parsers will be applied in order. Possible Formats:
                           {SortType.ExifTag:G}:{ExifTagId.DateTimeOriginal:G}                                       [Tries to use the exif tag 0x{ExifTagId.DateTimeOriginal:X} to get a date]
                           {SortType.ExifTag:G}:{ExifTagId.DateTimeDigitized:G}                                      [Tries to use the exif tag 0x{ExifTagId.DateTimeDigitized:X} to get a date]
                           {SortType.ExifTag:G}:{ExifTagId.DateTime:G}                                               [Tries to use the exif tag 0x{ExifTagId.DateTime:X} to get a date]
                           {SortType.QuickTimeMovieHeader:G}:{QuickTimeMovieHeader.CreationTime:G}                              [Tries to use the quick time movie header (mvhd) 'Creation time' to get a date]
                           {SortType.QuickTimeMovieHeader:G}:{QuickTimeMovieHeader.ModificationTime:G}                          [Tries to use the quick time movie header (mvhd) 'Modification time' to get a date]
                           {SortType.FileName:G}:<Regex with named capture groups year month and day>  [Tries to parse the file name using a regular expression to get a date]
                           """
        };

        public static readonly Option<string> FormatOption = new("--format")
        {
            Description = "Output directory structure (date format specifier separated by / )",
            DefaultValueFactory = _ => "yyyy/MM"
        };

        public static readonly Option<LogLevel> LogLevelOption = new("--log-level")
        {
            Description = "Log Level",
            DefaultValueFactory = _ => LogLevel.Information
        };

        public static readonly Option<bool> BeVerboseOption = new("-v", "--verbose")
        {
            Description = "Same as --log-level Trace"
        };

        public static readonly Option<bool> PreferFileNameParsingOption =
            new("--fast-scan", "--prefer-file-name-parsing")
            {
                Description =
                    "Prefer FileName parsers over metadata-based parsers (which is significantly faster since parsing a file name which already is in memory doesn't use I/O)"
            };

        public static readonly Option<DateTime> SkipParserWhenDateBeforeOption = new("--skip-parser-when-before")
        {
            Description = "Skip the result of a parser when the resulting date is earlier",
            DefaultValueFactory = _ => DateTime.Parse("1950-01-01")
        };

        public static readonly Option<DateTime> SkipParserWhenDateAfterOption = new("--skip-parser-when-after")
        {
            Description = "Skip the result of a parser when the resulting date is later",
            DefaultValueFactory = _ => DateTime.Now.Date.AddYears(1)
        };

        public static readonly Option<bool> IsDryRunOption = new("--dry-run")
        {
            Description = "Don't move or copy any files, just print the planned operations to a file",
            DefaultValueFactory = _ => false
        };

        public static readonly Option<bool> UseProgressBar = new("--progress-bar")
        {
            Description = "Show animated progress bar",
            DefaultValueFactory = _ => true
        };

        public static readonly Option<string> ProgressBarString = new("--progress-bar-chars")
        {
            Description = "Characters to use for rendering the progress bar",
            DefaultValueFactory = _ => " -=#"
        };

        public static readonly Option<FileInfo?> SummaryFilePathOption = new("--summary-path")
        {
            Description =
                "The path where a summary file should be saved. In case of a directory, the file name is generated."
        };

        public static readonly Option<bool> EscapeSummaryMarkdownTables = new("--escape-summary-tables")
        {
            Description = "Escape the content in Markdown tables in the summary file.",
            DefaultValueFactory = _ => true
        };

        public static readonly Option<ConflictReducerMode?> ConflictReducerModeOption = new("--conflict-reduction-mode")
        {
            Description = "How conflicting files are compared to skip duplicate equal files",
            DefaultValueFactory = _ => ConflictReducerMode.FileContent
        };

        public static readonly Option<DestinationConflictMode?> DestinationConflictModeOption =
            new("--destination-conflict-mode")
            {
                Description = "How conflicting files between source and destination are handled",
                DefaultValueFactory = _ => DestinationConflictMode.Joint
            };

        public static readonly Option<ConflictResolverMode?> ConflictResolverModeOption =
            new("--conflict-resolution-mode")
            {
                Description = "How conflicts are resolved",
                DefaultValueFactory = _ => ConflictResolverMode.PathHashRename
            };

        public static readonly Option<CaseSensitivityDetectionMode?> CaseSensitivityDetectionOption =
            new("--case-sensitivity")
            {
                Description = "Whether file names differing only in casing should be treated as equal or not",
                DefaultValueFactory = _ => CaseSensitivityDetectionMode.Auto
            };
    }
}