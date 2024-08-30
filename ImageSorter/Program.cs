// See https://aka.ms/new-console-template for more information


using System.CommandLine;
using ImageSorter;
using ImageSorter.Services.DateParser;
using ImageSorter.Services.FileHandling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var description = @"Sorts images chronologically in the directory structure year/month
Will use the following clues to determine a date for each file in order:

- Exif Tag DateTimeOriginal  (0x9003)
- Exif Tag DateTimeDigitized (0x9004)
- Exif Tag DateTime          (0x0132)
- Regex for file name: .*(20[0-9]{2}|19[0-9]{2})-(0[0-9]|1[0-9])-(0[0-9]|1[0-9]|2[0-9]|3[0-1]).* : detects yyyy-mm-dd
- Regex for file name: .*(20[0-9]{2}|19[0-9]{2})(0[0-9]|1[0-9])(0[0-9]|1[0-9]|2[0-9]|3[0-1]).*   : detects yyyymmdd
- Last edit date from file system
";

var rootCommand = new RootCommand(description: description);
var sourceArgument = new Argument<FileInfo>("source path", "The path of the source directory");
var destinationOption = new Option<FileInfo?>(aliases: new[] { "--dest" },
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
        SourcePath = parsedContext.GetValueForArgument(sourceArgument),
        DestinationPath = destPath ?? parsedContext.GetValueForArgument(sourceArgument),
        MoveFiles = parsedContext.GetValueForOption(inPlaceOption),
        FileEndings = parsedContext.GetValueForOption(fileEndingsOption),
        Overwrite = parsedContext.GetValueForOption(overwriteOption),
        From = parsedContext.GetValueForOption(fromOption),
        To = parsedContext.GetValueForOption(toOption),
        ScanParallel = parsedContext.GetValueForOption(parallelScanningOption),
        ProgressAt = parsedContext.GetValueForOption(progressOption)
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