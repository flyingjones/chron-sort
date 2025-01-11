using ImageSorter.Markdown.Model;
using ImageSorter.Markdown.Services;
using ImageSorter.Services.DateParser;
using ImageSorter.Services.FileHandling;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Services;

public partial class Sorter : ISorter
{
    private readonly ILogger<Sorter> _logger;
    private readonly IFileLoader _fileLoader;
    private readonly IDateParsingHandler _dateParsingHandler;
    private readonly IDestinationWriter _destinationWriter;
    private readonly IMarkdownFileWriter _markdownFileWriter;
    private readonly ISummarizeService _summarizeService;

    public Sorter(
        ILogger<Sorter> logger,
        IFileLoader fileLoader,
        IDateParsingHandler dateParsingHandler,
        IDestinationWriter destinationWriter,
        IMarkdownFileWriter markdownFileWriter,
        ISummarizeService summarizeService)
    {
        _logger = logger;
        _fileLoader = fileLoader;
        _dateParsingHandler = dateParsingHandler;
        _destinationWriter = destinationWriter;
        _markdownFileWriter = markdownFileWriter;
        _summarizeService = summarizeService;
    }

    public async Task PerformSorting(bool moveFiles, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scanning Source Directory");
        var filesToProcess = _fileLoader.GetFilePaths(out var allFiles);
        LogScanSummary(filesToProcess, allFiles);

        _logger.LogInformation("Parsing Dates");
        var writeQueue = (await _dateParsingHandler.ScanFiles(filesToProcess, cancellationToken)).ToList();
        LogSortSummary(writeQueue);

        _logger.LogInformation("Writing results");
        ICollection<FileOperationResult> writeSummary;
        if (!moveFiles)
        {
            writeSummary = await _destinationWriter.CopyFiles(writeQueue, cancellationToken);
        }
        else
        {
            writeSummary = _destinationWriter.MoveFiles(writeQueue, cancellationToken);
        }

        LogWriteSummary(writeSummary);

        _logger.LogInformation("Finished");
    }

    private void LogScanSummary(string[] filesToProcess, string[] allFiles)
    {
        var fileScanSummary = _summarizeService.SummarizeFileScan(filesToProcess, allFiles);
        _logger.LogInformation("File Scan Summary {FileScanSummary}", Environment.NewLine + fileScanSummary.Render(false));
        _markdownFileWriter.WriteHeading(MarkdownHeading.H2, "File Scan Summary");
        _markdownFileWriter.WriteTable(fileScanSummary);
        _markdownFileWriter.WriteLine();
    }

    private void LogSortSummary(ICollection<WriteQueueItem> writeQueue)
    {
        var conflictSummary = _summarizeService.SummarizeConflicts(writeQueue);
        _logger.LogInformation("Sorting Summary {ConflictTable}", Environment.NewLine + conflictSummary.Render(false));
        _markdownFileWriter.WriteHeading(MarkdownHeading.H2, "Sort Summary");
        _markdownFileWriter.WriteHeading(MarkdownHeading.H3, "Conflict Summary");
        _markdownFileWriter.WriteTable(conflictSummary);
        

        var conflictDescription = _summarizeService.DescribeConflicts(writeQueue);

        if (conflictDescription.Count != 0)
        {
            _markdownFileWriter.WriteHeading(MarkdownHeading.H3, "Conflicts");
        }

        foreach (var conflict in conflictDescription)
        {
            _markdownFileWriter.WriteHeading(MarkdownHeading.H4, conflict.Key);
            _markdownFileWriter.WriteTable(conflict.Value);
        }
    }

    private void LogWriteSummary(ICollection<FileOperationResult> results)
    {
        var sortSummaryTable = _summarizeService.SummarizeWriteResults(results);
        _logger.LogInformation("Write Summary {SortSummary}", Environment.NewLine + sortSummaryTable.Render(false));
        _markdownFileWriter.WriteHeading(MarkdownHeading.H2, "Write Summary");
        _markdownFileWriter.WriteTable(sortSummaryTable);

        var writeDescriptions = _summarizeService.DescribeWrites(results);
        foreach (var writeDescription in writeDescriptions)
        {
            _markdownFileWriter.WriteHeading(MarkdownHeading.H3, writeDescription.Key);
            _markdownFileWriter.WriteTable(writeDescription.Value);
        }
    }
}