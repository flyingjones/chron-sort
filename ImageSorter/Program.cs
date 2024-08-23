// See https://aka.ms/new-console-template for more information


using System.CommandLine;
using ImageSorter;
using ImageSorter.Services.DateParser;
using ImageSorter.Services.FileHandling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var rootCommand = new RootCommand(description: "sorts images based on the taken date from their meta data");
var sourcePathOption =
    new Option<FileInfo?>(aliases: new[] { "--input", "-i" }, description: "The path of the source directory");
var destinationPathOption = new Option<FileInfo?>(aliases: new[] { "--dest", "-d" },
    description: "The path of the destination directory");
var fileEndingsOption = new Option<string[]>(aliases: new[] { "--types", "-t" },
    description: "space seperated list of file endings to copy");
var overwriteOption =
    new Option<bool>(aliases: new[] { "--overwrite" }, description: "Overwrite files in destination");
var parallelScanningOption =
    new Option<bool>(aliases: new[] { "--scan-parallel" }, description: "Perform the scan part in parallel");
var fromOption = new Option<DateTime?>(aliases: new[] { "--from" }, description: "min date for files to sort");
var toOption = new Option<DateTime?>(aliases: new[] { "--to" }, description: "max date for files to sort");
var progressOption = new Option<int?>(aliases: new[] { "--progress-at" },
    description: "written file count after which a progress update is printed", getDefaultValue: () => 1000);
rootCommand.AddOption(sourcePathOption);
rootCommand.AddOption(destinationPathOption);
rootCommand.AddOption(fileEndingsOption);
rootCommand.AddOption(overwriteOption);
rootCommand.AddOption(fromOption);
rootCommand.AddOption(toOption);
rootCommand.AddOption(parallelScanningOption);
rootCommand.AddOption(progressOption);

rootCommand.SetHandler(async (context) =>
{
    var parsedContext = context.ParseResult;
    var runConfig = new RunConfiguration
    {
        SourcePath = parsedContext.GetValueForOption(sourcePathOption)!,
        DestinationPath = parsedContext.GetValueForOption(destinationPathOption)!,
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
    await destinationWriter.CopyFiles(writeQueue.ToList(), context.GetCancellationToken());

    logger.LogInformation("Finished sorting");
});

await rootCommand.InvokeAsync(args);