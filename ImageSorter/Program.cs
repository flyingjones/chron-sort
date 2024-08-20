// See https://aka.ms/new-console-template for more information


using System.Collections.Concurrent;
using System.CommandLine;
using System.Drawing;
using System.Net.Mime;
using ImageSorter;
using System.Drawing.Imaging;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

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
    var sourcePath = context.ParseResult.GetValueForOption(sourcePathOption);
    var destPath = context.ParseResult.GetValueForOption(destinationPathOption);
    var fileEndingsToSupport = context.ParseResult.GetValueForOption(fileEndingsOption);
    
    Console.WriteLine("--- Started sorting images");
    Console.WriteLine($"src: {sourcePath!.FullName}");
    Console.WriteLine($"dest: {destPath!.FullName}");

    var files = Directory.GetFiles(sourcePath!.FullName, "*", searchOption: SearchOption.AllDirectories);
    
    
    Console.WriteLine("--- File Endings Found:");
    var fileEndings = files.Select(x => x.Split(".")[^1]).Distinct().ToArray();
    foreach (var fileEnding in fileEndings)
    {
        Console.WriteLine(fileEnding);
    }

    var filesToProcess = fileEndingsToSupport == null || fileEndingsToSupport.Length == 0 ? 
        files : 
        files.Where(x => fileEndingsToSupport.Contains(x.FileEnding())).ToArray();
    
    Console.WriteLine($"Sorting {filesToProcess.Length} files");

    var destinationWriter = new DestinationWriter(destPath.FullName);

    var unsortedFiles = new ConcurrentStack<string>();

    var writeQueue = new ConcurrentQueue<WriteQueueItem>();

    await Parallel.ForEachAsync(filesToProcess, context.GetCancellationToken(), async (filePath, cancellationToken ) =>
    {
        try
        {
            var dateTaken = await DateParser.GetDateTaken(filePath);
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
    
    Console.WriteLine($"Scanned {writeQueue.Count} files successfully");

    var yearGroups = writeQueue.OrderBy(x => x.DateTaken).GroupBy(x => x.DateTaken.Year);

    foreach (var yearGroup in yearGroups)
    {
        Console.WriteLine($"Writing year {yearGroup.Key}");
        foreach (var item in yearGroup)
        {
            destinationWriter.CopyFile(item.FilePath, item.DateTaken);
        }
    }
    
    var foundFiles = Directory.GetFiles(destPath!.FullName, "*", searchOption: SearchOption.AllDirectories);
    
    Console.WriteLine($"Found {foundFiles.Length} files in output dir");
});

await rootCommand.InvokeAsync(args);