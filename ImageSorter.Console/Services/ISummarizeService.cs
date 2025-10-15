using ImageSorter.DateParsing.Abstractions.Model.MetaData;
using ImageSorter.Markdown.Abstractions.Model;
using ImageSorter.ResultWriting.Abstractions.Model;
using ImageSorter.Sorting.Abstractions.Model;

namespace ImageSorter.Services;

/// <summary>
/// Service to create <see cref="MarkdownTable"/> for summarizing results of sorting steps
/// </summary>
public interface ISummarizeService
{
    /// <summary>
    /// Creates a summary of a file scan as a <see cref="MarkdownTable"/>
    /// </summary>
    /// <remarks>
    /// The table has the following columns:
    ///
    /// <code>
    /// | File Ending | Used Count                        | Ignored Count                                         |
    /// | File Ending | Number of files in filesToProcess | Number of files in allFiles but not in filesToProcess |
    /// </code>
    ///
    /// And the last row will be for all file endings.
    /// </remarks>
    /// <param name="filesToProcess">The files matched by the specified file filter</param>
    /// <param name="allFiles">All files found in the directory</param>
    MarkdownTable SummarizeFileScan(string[] filesToProcess, string[] allFiles);

    /// <summary>
    /// Creates a summary <see cref="MarkdownTable"/> based on the <paramref name="reducedSortingConflicts"/> grouped by year
    /// </summary>
    /// <remarks>
    /// The table has the following columns:
    /// 
    /// <code>
    /// | Year | Total | Non-Conflicting | Conflicting | Present at Destination | Remaining Conflicting after Reduction | Discarded | Remaining Total |
    /// </code>
    /// </remarks>
    MarkdownTable SummarizeConflicts(ReducedSortingConflictSummary reducedSortingConflicts);

    /// <summary>
    /// Creates a summary <see cref="MarkdownTable"/> for the conflict resolution
    /// </summary>
    /// <remarks>
    /// The table has the following columns:
    /// 
    /// <code>
    /// | Year | Total | Non-Conflicting | Conflicting | Renamed | Discarded | Total Writes |
    /// </code>
    /// </remarks>
    MarkdownTable SummarizeConflictResolution(ICollection<SortedFilePath> filesToWrite,
        ICollection<SortedFilePath> discardedFiles, SortingConflictSummary sortingConflictsAfterReduction);

    /// <summary>
    /// Creates a summary <see cref="MarkdownTable"/> based on the <paramref name="fileOperationResults"/> grouped by year
    /// </summary>
    /// <remarks>
    /// The table has the following columns:
    ///
    /// <code>
    /// | Year | Total | Error | Skipped | Already Sorted | Success | Success (Overwritten) |
    /// </code>
    /// </remarks>
    MarkdownTable SummarizeWriteResults(ICollection<FileWriteResultDto> fileOperationResults);
}