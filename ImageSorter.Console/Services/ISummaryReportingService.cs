using ImageSorter.DateParsing.Abstractions.Model.MetaData;
using ImageSorter.Services.FileHandling;
using ImageSorter.Sorting.Abstractions.Model;

namespace ImageSorter.Services;

/// <summary>
/// Service to report summaries of a sorting step.
/// </summary>
public interface ISummaryReportingService
{
    void ReportScanSummary(string[] filesToProcess, string[] allFiles);

    void ReportSortSummary(ICollection<ParsedFileResult> writeQueue);

    void ReportWriteSummary(ICollection<FileOperationResult> results);

    void ReportConflictSummary(ReducedSortingConflictSummary result);
}