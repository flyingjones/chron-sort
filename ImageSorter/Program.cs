// See https://aka.ms/new-console-template for more information


using System.Collections.Concurrent;
using System.CommandLine;
using System.Diagnostics;
using ImageSorter;
using ImageSorter.Logging;
using ImageSorter.Services.DateParser;
using ImageSorter.Services.DateParser.MetaData;
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
var fromOption = new Option<DateTime?>(aliases: new[] { "--from" }, description: "min date for files to sort");
var toOption = new Option<DateTime?>(aliases: new[] { "--to" }, description: "max date for files to sort");
rootCommand.AddOption(sourcePathOption);
rootCommand.AddOption(destinationPathOption);
rootCommand.AddOption(fileEndingsOption);
rootCommand.AddOption(overwriteOption);
rootCommand.AddOption(fromOption);
rootCommand.AddOption(toOption);

rootCommand.SetHandler(async (context) =>
{
    var runConfig = new RunConfiguration
    {
        SourcePath = context.ParseResult.GetValueForOption(sourcePathOption)!,
        DestinationPath = context.ParseResult.GetValueForOption(destinationPathOption)!,
        FileEndings = context.ParseResult.GetValueForOption(fileEndingsOption),
        Overwrite = context.ParseResult.GetValueForOption(overwriteOption),
        From = context.ParseResult.GetValueForOption(fromOption),
        To = context.ParseResult.GetValueForOption(toOption)
    };

    var serviceCollection = runConfig.SetupServices();

    var serviceProvider = serviceCollection.BuildServiceProvider();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    
    RunConfigurationHelper.LogRunConfiguration(logger, runConfig);
    
    var files = Directory.GetFiles(runConfig.SourcePath.FullName, "*", searchOption: SearchOption.AllDirectories);

    var filesToProcess = runConfig.FileEndings == null || runConfig.FileEndings.Length == 0 ? 
        files : 
        files.Where(x => runConfig.FileEndings.Contains(x.FileEnding())).ToArray();
    
    logger.LogInformation("Found {fileCount} files to sort", filesToProcess.Length);

    // TODO clean up
    
    var writeQueue = new ConcurrentQueue<WriteQueueItem>();
    
    logger.LogInformation("Starting scan of all files");

    var dateParser = serviceProvider.GetRequiredService<IDateParser>();

    await Parallel.ForEachAsync(filesToProcess, context.GetCancellationToken(), async (filePath, cancellationToken ) =>
    {
        try
        {
            var dateTaken = await dateParser.ParseDate(filePath);
            writeQueue.Enqueue(new WriteQueueItem
            {
                DateTaken = dateTaken,
                FilePath = filePath
            });
        }
        catch (Exception exception)
        {
            // TODO logger
            Console.WriteLine(filePath);
            Console.WriteLine($"Something went wrong: {exception.Message}");
        }
    });
    
    logger.LogInformation("Scanned {writeQueueCount} files successfully", writeQueue.Count);

    var yearGroups = writeQueue
        .Where(x => runConfig.From == null || x.DateTaken >= runConfig.From)
        .Where(x => runConfig.To == null || x.DateTaken <= runConfig.To)
        .OrderBy(x => x.DateTaken).GroupBy(x => x.DateTaken.Year);
    
    logger.LogInformation("Sorted {writeQueueCount} files in memory", writeQueue.Count);

    var destinationWriter = serviceProvider.GetRequiredService<IDestinationWriter>();
    foreach (var yearGroup in yearGroups)
    {
        logger.LogInformation("Writing year {year} ({fileCount} files)", yearGroup.Key, yearGroup.Count());
        foreach (var item in yearGroup)
        {
            await destinationWriter.CopyFile(item.FilePath, item.DateTaken, context.GetCancellationToken());
        }
    }
    
    var foundFiles = Directory.GetFiles(runConfig.DestinationPath.FullName, "*", searchOption: SearchOption.AllDirectories);
    
    logger.LogInformation("Found {foundFilesCount} files in output dir", foundFiles.Length);
});

await rootCommand.InvokeAsync(args);