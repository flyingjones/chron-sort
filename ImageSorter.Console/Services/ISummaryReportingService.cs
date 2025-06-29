using ImageSorter.ResultWriting.Abstractions.Model;
using ImageSorter.Sorting.Abstractions.Model;

namespace ImageSorter.Services;

/// <summary>
/// Service to report summaries of a sorting step.
/// </summary>
public interface ISummaryReportingService
{
    void ReportScanSummary(string[] filesToProcess, string[] allFiles);
    
    void ReportWriteSummary(ICollection<FileWriteResultDto> results);

    void ReportConflictSummary(ReducedSortingConflictSummary result);
}