using ImageSorter.DateParsing.Abstractions.Model.MetaData;
using ImageSorter.Markdown.Abstractions.Model;
using ImageSorter.Markdown.Abstractions.Services;
using ImageSorter.Markdown.Services;
using ImageSorter.ResultWriting.Abstractions.Model;
using ImageSorter.Sorting.Abstractions.Model;
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

    public void ReportWriteSummary(ICollection<FileWriteResultDto> results)
    {
        var sortSummaryTable = _summarizeService.SummarizeWriteResults(results);
        _logger.LogInformation(
            "Write Summary {SortSummary}",
            Environment.NewLine + _markdownTableRenderEngine.Render(sortSummaryTable, false));
        _markdownFileWriter.WriteHeading(MarkdownHeading.H2, "Write Summary");
        _markdownFileWriter.WriteTable(sortSummaryTable);

        // var writeDescriptions = _summarizeService.DescribeWrites(results);
        // foreach (var writeDescription in writeDescriptions)
        // {
        //     _markdownFileWriter.WriteHeading(MarkdownHeading.H3, writeDescription.Key);
        //     _markdownFileWriter.WriteTable(writeDescription.Value);
        // }
    }

    public void ReportConflictSummary(ReducedSortingConflictSummary result)
    {
        var conflictSummaryTable = _summarizeService.SummarizeConflicts(result);

        _logger.LogInformation(
            "Sort Conflict Summary {SortConflictSummary}",
            Environment.NewLine + _markdownTableRenderEngine.Render(conflictSummaryTable, false));

        _markdownFileWriter.WriteHeading(MarkdownHeading.H2, "Sort Conflict Summary");
        _markdownFileWriter.WriteTable(conflictSummaryTable);
    }

    public void ReportConflictResolution(ICollection<SortedFilePath> filesToWrite,
        ICollection<SortedFilePath> discardedFiles, SortingConflictSummary sortingConflictsAfterReduction)
    {
        var conflictSummaryTable =
            _summarizeService.SummarizeConflictResolution(filesToWrite, discardedFiles, sortingConflictsAfterReduction);

        _logger.LogInformation(
            "Sort Conflict Resolution Summary {SortConflictSummary}",
            Environment.NewLine + _markdownTableRenderEngine.Render(conflictSummaryTable, false));

        _markdownFileWriter.WriteHeading(MarkdownHeading.H2, "Sort Conflict Resolution Summary");
        _markdownFileWriter.WriteTable(conflictSummaryTable);
    }
}