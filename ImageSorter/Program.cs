// See https://aka.ms/new-console-template for more information


using System.CommandLine;
using ImageSorter;
using ImageSorter.Services.DateParser;
using ImageSorter.Services.DateParser.MetaData;
using ImageSorter.Services.FileHandling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var description = $"""
                   Sorts images chronologically in the directory structure year/month
                   Default sort configuration (change with --configure):
                   {string.Join(Environment.NewLine, SortConfigurationFactory.DefaultSorting)}
                   """;

var rootCommand = new RootCommand(description: description);
var sourceArgument = new Argument<FileInfo>("source path", "The path of the source directory");
var destinationOption = new Option<FileInfo?>(aliases: new[] { "--dest", "--out" },
    description: "The path of the destination directory (required if not --move)");
var inPlaceOption = new Option<bool>(aliases: new[] { "--move" }, description: "Move files instead of copy",
    getDefaultValue: () => false);
var fileEndingsOption = new Option<string[]>(aliases: new[] { "--types", "-t" },
    description: "Space seperated list of file endings to sort");
var overwriteOption =
    new Option<bool>(aliases: new[] { "--overwrite" }, description: "Overwrite files in destination",
        getDefaultValue: () => false);
var parallelScanningOption =
    new Option<bool>(aliases: new[] { "--scan-parallel" }, description: "Perform the scan part in parallel",
        getDefaultValue: () => false);
var fromOption = new Option<DateTime?>(aliases: new[] { "--from" }, description: "Minimum date for files to sort");
var toOption = new Option<DateTime?>(aliases: new[] { "--to" }, description: "Maximum date for files to sort");
var progressOption = new Option<int?>(aliases: new[] { "--progress-at" },
    description: "Processed file count after which a progress update is printed", getDefaultValue: () => 1000);
var sortConfiguration = new Option<string[]>(
    aliases: new[] { "-c", "--configure" },
    description: $"""
                  Custom sort configuration. Parsers will be applied in order. Possible Formats:
                  {SortType.ExifTag:G}:{ExifTagId.DateTimeOriginal:G}                                       [Tries to use the exif tag 0x{ExifTagId.DateTimeOriginal:X} to get a date]
                  {SortType.ExifTag:G}:{ExifTagId.DateTimeDigitized:G}                                      [Tries to use the exif tag 0x{ExifTagId.DateTimeDigitized:X} to get a date]
                  {SortType.ExifTag:G}:{ExifTagId.DateTime:G}                                               [Tries to use the exif tag 0x{ExifTagId.DateTime:X} to get a date]
                  {SortType.FileName:G}:<Regex with named capture groups year month and day>  [Tries to parse the file name using a regular expression to get a date]
                  """);
var logLevelOption = new Option<LogLevel>(aliases: new[] { "--log-level" }, description: "Log Level",
    getDefaultValue: () => LogLevel.Information);
var verboseOption = new Option<bool>(new[] { "-v", "--verbose" }, description: "Same as --log-level Trace");
var preferFileNameParsingOption = new Option<bool>(new[] { "--fast-scan", "--prefer-file-name-parsing" },
    description:
    "Prefer FileName parsers over ExifTag parsers (which is significantly faster since parsing a file name which already is in memory doesn't use I/O)");

var skipParserWhenDateBeforeOption = new Option<DateTime>(
    aliases: new[] { "--skip-parser-when-before" },
    description: "Skip the result of a parser when the resulting date is earlier",
    getDefaultValue: () => DateTime.Parse("1900-01-01"));

var skipParserWhenDateAfterOption = new Option<DateTime>(
    aliases: new[] { "--skip-parser-when-after" },
    description: "Skip the result of a parser when the resulting date is later",
    getDefaultValue: () => DateTime.Now.Date.AddYears(1));

rootCommand.AddOption(skipParserWhenDateBeforeOption);
rootCommand.AddOption(skipParserWhenDateAfterOption);

rootCommand.AddOption(sortConfiguration);
rootCommand.AddOption(preferFileNameParsingOption);
rootCommand.AddOption(logLevelOption);
rootCommand.AddOption(verboseOption);
rootCommand.AddArgument(sourceArgument);
rootCommand.AddOption(destinationOption);
rootCommand.AddOption(inPlaceOption);
rootCommand.AddOption(fileEndingsOption);
rootCommand.AddOption(overwriteOption);
rootCommand.AddOption(fromOption);
rootCommand.AddOption(toOption);
rootCommand.AddOption(parallelScanningOption);
rootCommand.AddOption(progressOption);

rootCommand.SetHandler(async (context) =>
{
    var parsedContext = context.ParseResult;

    var destPath = parsedContext.GetValueForOption(destinationOption);
    var isInPlace = parsedContext.GetValueForOption(inPlaceOption);

    if (destPath == null && !isInPlace)
    {
        throw new ArgumentException("no destination path provided but in-place was not set");
    }

    var runConfig = new RunConfiguration
    {
        SortConfiguration = parsedContext.GetValueForOption(sortConfiguration),
        PreferFileNameParsing = parsedContext.GetValueForOption(preferFileNameParsingOption),
        SourcePath = parsedContext.GetValueForArgument(sourceArgument),
        DestinationPath = destPath ?? parsedContext.GetValueForArgument(sourceArgument),
        MoveFiles = parsedContext.GetValueForOption(inPlaceOption),
        FileEndings = parsedContext.GetValueForOption(fileEndingsOption),
        Overwrite = parsedContext.GetValueForOption(overwriteOption),
        From = parsedContext.GetValueForOption(fromOption),
        To = parsedContext.GetValueForOption(toOption),
        ScanParallel = parsedContext.GetValueForOption(parallelScanningOption),
        ProgressAt = parsedContext.GetValueForOption(progressOption),
        LogLevel = parsedContext.GetValueForOption(verboseOption)
            ? LogLevel.Trace
            : parsedContext.GetValueForOption(logLevelOption),
        SkipParserBefore = parsedContext.GetValueForOption(skipParserWhenDateBeforeOption),
        SkipParserAfter = parsedContext.GetValueForOption(skipParserWhenDateAfterOption)
    };

    var serviceProvider = runConfig.SetupServices().BuildServiceProvider();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

    RunConfigurationHelper.LogRunConfiguration(logger, runConfig);

    var filesToProcess = serviceProvider.GetRequiredService<IFileLoader>().GetFilePaths();

    var writeQueue = await serviceProvider.GetRequiredService<IDateParsingHandler>()
        .ScanFiles(filesToProcess, context.GetCancellationToken());

    var destinationWriter = serviceProvider.GetRequiredService<IDestinationWriter>();

    if (!runConfig.MoveFiles)
    {
        await destinationWriter.CopyFiles(writeQueue, context.GetCancellationToken());
    }
    else
    {
        destinationWriter.MoveFiles(writeQueue, context.GetCancellationToken());
    }

    logger.LogInformation("Finished sorting");
});

await rootCommand.InvokeAsync(args);