// See https://aka.ms/new-console-template for more information


using System.Collections.Concurrent;
using System.CommandLine;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ImageSorter;
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
rootCommand.AddOption(sourcePathOption);
rootCommand.AddOption(destinationPathOption);
rootCommand.AddOption(fileEndingsOption);
Regex r = new Regex(":");

rootCommand.SetHandler(async (context) =>
{
    var stopWatch = new Stopwatch();
    stopWatch.Start();
    
    var sourcePath = context.ParseResult.GetValueForOption(sourcePathOption);
    var destPath = context.ParseResult.GetValueForOption(destinationPathOption);
    var fileEndingsToSupport = context.ParseResult.GetValueForOption(fileEndingsOption);

    var serviceCollection = new ServiceCollection();

    serviceCollection.AddMetaDataParsing();
    serviceCollection.AddSingleton<IFileNameDateParser>(
        new FilenameDateParser(".*(20[0-9]{2}|19[0-9]{2})-(0[0-9]|1[0-9])-(0[0-9]|1[0-9]|2[0-9]|3[0-1]).*", 0));
    serviceCollection.AddSingleton<IFileNameDateParser>(
        new FilenameDateParser(".*(20[0-9]{2}|19[0-9]{2})(0[0-9]|1[0-9])(0[0-9]|1[0-9]|2[0-9]|3[0-1]).*", 1));
    serviceCollection.AddDateParsing();
    serviceCollection.AddLogging(builder => builder
        .AddConsole(opt => opt.FormatterName = "stopwatchLogFormatter")
        .AddConsoleFormatter<StopwatchLogFormatter, StopwatchLogFormatterOptions>(opt => opt.Stopwatch = stopWatch));
    serviceCollection.AddDestinationWriter(destPath!.FullName, false);

    var serviceProvider = serviceCollection.BuildServiceProvider();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Starting image sort");
    logger.LogInformation("source: {sourcePath}, destination: {destPath}", sourcePath!.FullName, destPath!.FullName);

    if (fileEndingsToSupport?.Length > 0)
    {
        var fileEndingString = string.Join(", ", fileEndingsToSupport);
        logger.LogInformation("Sorting only files with endings [{fileEndingString}]", fileEndingString);
    }
    
    var files = Directory.GetFiles(sourcePath.FullName, "*", searchOption: SearchOption.AllDirectories);

    
    // Console.WriteLine("--- File Endings Found:");
    // var fileEndings = files.Select(x => x.Split(".")[^1]).Distinct().ToArray();
    // foreach (var fileEnding in fileEndings)
    // {
    //     Console.WriteLine(fileEnding);
    // }

    var filesToProcess = fileEndingsToSupport == null || fileEndingsToSupport.Length == 0 ? 
        files : 
        files.Where(x => fileEndingsToSupport.Contains(x.FileEnding())).ToArray();
    
    logger.LogInformation("Found {fileCount} files to sort", filesToProcess.Length);

    // TODO clean up
    
    var unsortedFiles = new ConcurrentStack<string>();

    var writeQueue = new ConcurrentQueue<WriteQueueItem>();
    
    logger.LogInformation("Starting scan of all files");

    var dateParser = serviceProvider.GetRequiredService<IDateParser>();
    // foreach (var filePath in filesToProcess)
    // {
    //     try
    //     {
    //         var dateTaken = await dateParser.ParseDate(filePath);
    //         writeQueue.Enqueue(new WriteQueueItem
    //         {
    //             DateTaken = dateTaken,
    //             FilePath = filePath
    //         });
    //     }
    //     catch (Exception exception)
    //     {
    //         Console.WriteLine(filePath);
    //         Console.WriteLine($"Something went wrong: {exception.Message}");
    //         unsortedFiles.Push(filePath);
    //     }
    // }
    //
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
            Console.WriteLine(filePath);
            Console.WriteLine($"Something went wrong: {exception.Message}");
            unsortedFiles.Push(filePath);
        }
    });
    
    logger.LogInformation("Scanned {writeQueueCount} files successfully", writeQueue.Count);

    var yearGroups = writeQueue.OrderBy(x => x.DateTaken).GroupBy(x => x.DateTaken.Year);
    
    logger.LogInformation("Sorted {writeQueueCount} files in memory", writeQueue.Count);

    var destinationWriter = serviceProvider.GetRequiredService<IDestinationWriter>();
    foreach (var yearGroup in yearGroups)
    {
        logger.LogInformation("Writing year {year} ({fileCount} files)", yearGroup.Key, yearGroup.Count());
        foreach (var item in yearGroup)
        {
            destinationWriter.CopyFile(item.FilePath, item.DateTaken);
        }
    }
    
    var foundFiles = Directory.GetFiles(destPath!.FullName, "*", searchOption: SearchOption.AllDirectories);
    
    logger.LogInformation("Found {foundFilesCount} files in output dir", foundFiles.Length);
});

await rootCommand.InvokeAsync(args);