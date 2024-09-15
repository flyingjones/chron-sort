using System.CommandLine;
using System.CommandLine.Parsing;
using ImageSorter.DependencyInjection;
using ImageSorter.Services.DateParser.MetaData.ExifTags;
using ImageSorter.Services.DateParser.MetaData.QuickTimeMovieHeaders;
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
        rootCommand.AddArgument(Arguments.SourcePathArgument);

        // options
        // main settings
        rootCommand.AddOption(Options.DestinationPathOption);
        rootCommand.AddOption(Options.MoveOption);
        rootCommand.AddOption(Options.OverwriteOption);
        rootCommand.AddOption(Options.IsDryRunOption);
        rootCommand.AddOption(Options.FormatOption);
        // parser config
        rootCommand.AddOption(Options.SortConfigurationOption);
        rootCommand.AddOption(Options.SkipParserWhenDateBeforeOption);
        rootCommand.AddOption(Options.SkipParserWhenDateAfterOption);
        // file filter
        rootCommand.AddOption(Options.FileEndingFilterOption);
        rootCommand.AddOption(Options.UseFromDateFilterOption);
        rootCommand.AddOption(Options.UseToDateFilterOption);
        // optimizations
        rootCommand.AddOption(Options.PreferFileNameParsingOption);
        rootCommand.AddOption(Options.UseParallelScanningOption);
        // logging
        rootCommand.AddOption(Options.LogLevelOption);
        rootCommand.AddOption(Options.BeVerboseOption);
        rootCommand.AddOption(Options.ProgressReportingIntervalOption);

        return rootCommand;
    }

    public static RunConfiguration ParseRunConfiguration(ParseResult parsedContext)
    {
        var destPath = parsedContext.GetValueForOption(Options.DestinationPathOption);
        var isMoveFiles = parsedContext.GetValueForOption(Options.MoveOption);

        if (destPath == null && !isMoveFiles)
        {
            throw new ArgumentException("no destination path provided but in-place was not set");
        }

        var runConfig = new RunConfiguration
        {
            SourcePath = parsedContext.GetValueForArgument(Arguments.SourcePathArgument),
            SortConfiguration = parsedContext.GetValueForOption(Options.SortConfigurationOption),
            PreferFileNameParsing = parsedContext.GetValueForOption(Options.PreferFileNameParsingOption),
            DestinationPath = destPath ?? parsedContext.GetValueForArgument(Arguments.SourcePathArgument),
            MoveFiles = isMoveFiles,
            FileEndings = parsedContext.GetValueForOption(Options.FileEndingFilterOption),
            Overwrite = parsedContext.GetValueForOption(Options.OverwriteOption),
            From = parsedContext.GetValueForOption(Options.UseFromDateFilterOption),
            To = parsedContext.GetValueForOption(Options.UseToDateFilterOption),
            ScanParallel = parsedContext.GetValueForOption(Options.UseParallelScanningOption),
            ProgressAt = parsedContext.GetValueForOption(Options.ProgressReportingIntervalOption),
            LogLevel = parsedContext.GetValueForOption(Options.BeVerboseOption)
                ? LogLevel.Trace
                : parsedContext.GetValueForOption(Options.LogLevelOption),
            SkipParserBefore = parsedContext.GetValueForOption(Options.SkipParserWhenDateBeforeOption),
            SkipParserAfter = parsedContext.GetValueForOption(Options.SkipParserWhenDateAfterOption),
            IsDryRun = parsedContext.GetValueForOption(Options.IsDryRunOption),
            OutputFormat = parsedContext.GetValueForOption(Options.FormatOption)
        };
        return runConfig;
    }

    private static class Arguments
    {
        public static readonly Argument<FileInfo> SourcePathArgument = new (
            name: "source path",
            description: "The path of the source directory");
    }

    private static class Options
    {
        public static readonly Option<FileInfo?> DestinationPathOption = new(
            aliases: new[] { "--dest", "--out" },
            description: "The path of the destination directory (required if not --move)");

        public static readonly Option<bool> MoveOption = new(
            aliases: new[] { "--move" },
            description: "Move files instead of copy",
            getDefaultValue: () => false);

        public static readonly Option<string[]> FileEndingFilterOption = new(
            aliases: new[] { "--types", "-t" },
            description: "Space seperated list of file endings to sort");

        public static readonly Option<bool> OverwriteOption = new(
            aliases: new[] { "--overwrite" },
            description: "Overwrite files in destination",
            getDefaultValue: () => false);

        public static readonly Option<bool> UseParallelScanningOption = new(
            aliases: new[] { "--scan-parallel" },
            description: "Perform the scan part in parallel",
            getDefaultValue: () => false);

        public static readonly Option<DateTime?> UseFromDateFilterOption = new(
            aliases: new[] { "--from" },
            description: "Minimum date for files to sort");

        public static readonly Option<DateTime?> UseToDateFilterOption = new(
            aliases: new[] { "--to" },
            description: "Maximum date for files to sort");

        public static readonly Option<int?> ProgressReportingIntervalOption = new(
            aliases: new[] { "--progress-at" },
            description: "Processed file count after which a progress update is printed",
            getDefaultValue: () => 1000);

        public static readonly Option<string[]> SortConfigurationOption = new(
            aliases: new[] { "-c", "--configure" },
            description: $"""
                          Custom sort configuration. Parsers will be applied in order. Possible Formats:
                          {SortType.ExifTag:G}:{ExifTagId.DateTimeOriginal:G}                                       [Tries to use the exif tag 0x{ExifTagId.DateTimeOriginal:X} to get a date]
                          {SortType.ExifTag:G}:{ExifTagId.DateTimeDigitized:G}                                      [Tries to use the exif tag 0x{ExifTagId.DateTimeDigitized:X} to get a date]
                          {SortType.ExifTag:G}:{ExifTagId.DateTime:G}                                               [Tries to use the exif tag 0x{ExifTagId.DateTime:X} to get a date]
                          {SortType.QuickTimeMovieHeader:G}:{QuickTimeMovieHeader.CreationTime:G}                              [Tries to use the quick time movie header (mvhd) 'Creation time' to get a date]
                          {SortType.QuickTimeMovieHeader:G}:{QuickTimeMovieHeader.ModificationTime:G}                          [Tries to use the quick time movie header (mvhd) 'Modification time' to get a date]
                          {SortType.FileName:G}:<Regex with named capture groups year month and day>  [Tries to parse the file name using a regular expression to get a date]
                          """);

        public static readonly Option<string> FormatOption = new(
            aliases: new[]{"--format"},
            description: "Output directory structure (date format specifier separated by / )",
            getDefaultValue: () => "yyyy/MM");

        public static readonly Option<LogLevel> LogLevelOption = new(
            aliases: new[] { "--log-level" },
            description: "Log Level",
            getDefaultValue: () => LogLevel.Information);

        public static readonly Option<bool> BeVerboseOption = new(
            new[] { "-v", "--verbose" },
            description: "Same as --log-level Trace");

        public static readonly Option<bool> PreferFileNameParsingOption = new(
            new[] { "--fast-scan", "--prefer-file-name-parsing" },
            description:
            "Prefer FileName parsers over metadata-based parsers (which is significantly faster since parsing a file name which already is in memory doesn't use I/O)");

        public static readonly Option<DateTime> SkipParserWhenDateBeforeOption = new(
            aliases: new[] { "--skip-parser-when-before" },
            description: "Skip the result of a parser when the resulting date is earlier",
            getDefaultValue: () => DateTime.Parse("1900-01-01"));

        public static readonly Option<DateTime> SkipParserWhenDateAfterOption = new(
            aliases: new[] { "--skip-parser-when-after" },
            description: "Skip the result of a parser when the resulting date is later",
            getDefaultValue: () => DateTime.Now.Date.AddYears(1));

        public static readonly Option<bool> IsDryRunOption = new(
            aliases: new[] { "--dry-run" },
            description: "Don't move or copy any files, just print the planned operations to a file",
            getDefaultValue: () => false);
    }
}