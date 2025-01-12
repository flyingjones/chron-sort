using ImageSorter.Services.FileHandling;

namespace ImageSorter.Services;

/// <summary>
/// Service to report summaries of a sorting step.
/// </summary>
public interface ISummaryReportingService
{
    void ReportScanSummary(string[] filesToProcess, string[] allFiles);

    void ReportSortSummary(ICollection<WriteQueueItem> writeQueue);

    void ReportWriteSummary(ICollection<FileOperationResult> results);
}