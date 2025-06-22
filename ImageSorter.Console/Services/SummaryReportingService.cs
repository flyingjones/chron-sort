using ImageSorter.Markdown.Abstractions.Model;
using ImageSorter.Markdown.Abstractions.Services;
using ImageSorter.Markdown.Services;
using ImageSorter.Services.FileHandling;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Services;

/// <summary>
/// Implementation of <see cref="ISummaryReportingService"/> which writes to the log and the report file
/// </summary>
public class SummaryReportingService : ISummaryReportingService
{
    private readonly ILogger<SummaryReportingService> _logger;
    private readonly ISummarizeService _summarizeService;
    private readonly IMarkdownFileWriter _markdownFileWriter;
    private readonly IMarkdownTableRenderEngine _markdownTableRenderEngine;

    public SummaryReportingService(
        ILogger<SummaryReportingService> logger,
        ISummarizeService summarizeService,
        IMarkdownFileWriter markdownFileWriter,
        IMarkdownTableRenderEngine markdownTableRenderEngine)
    {
        _logger = logger;
        _summarizeService = summarizeService;
        _markdownFileWriter = markdownFileWriter;
        _markdownTableRenderEngine = markdownTableRenderEngine;
    }

    public void ReportScanSummary(string[] filesToProcess, string[] allFiles)
    {
        var fileScanSummary = _summarizeService.SummarizeFileScan(filesToProcess, allFiles);
        _logger.LogInformation(
            "File Scan Summary {FileScanSummary}",
            Environment.NewLine + _markdownTableRenderEngine.Render(fileScanSummary, false));
        _markdownFileWriter.WriteHeading(MarkdownHeading.H2, "File Scan Summary");
        _markdownFileWriter.WriteTable(fileScanSummary);
        _markdownFileWriter.WriteLine();
    }

    public void ReportSortSummary(ICollection<WriteQueueItem> writeQueue)
    {
        var conflictSummary = _summarizeService.SummarizeConflicts(writeQueue);
        _logger.LogInformation(
            "Sorting Summary {ConflictTable}",
            Environment.NewLine + _markdownTableRenderEngine.Render(conflictSummary, false));
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

    public void ReportWriteSummary(ICollection<FileOperationResult> results)
    {
        var sortSummaryTable = _summarizeService.SummarizeWriteResults(results);
        _logger.LogInformation(
            "Write Summary {SortSummary}",
            Environment.NewLine + _markdownTableRenderEngine.Render(sortSummaryTable, false));
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