using ImageSorter.DateParsing.Abstractions.Model.MetaData;
using ImageSorter.Markdown.Abstractions.Model;
using ImageSorter.ResultWriting.Abstractions.Model;
using ImageSorter.Services.FileHandling;
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
    /// 
    /// </summary>
    /// <remarks>
    /// The table has the following columns:
    /// 
    /// <code>
    /// TBD
    /// </code>
    /// </remarks>
    MarkdownTable SummarizeConflicts(ReducedSortingConflictSummary reducedSortingConflicts);

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

    /// <summary>
    /// Describe each write operation as a row in a <see cref="MarkdownTable"/> grouped by the result directory path.
    /// </summary>
    /// <remarks>
    /// The key will be the result directory path
    ///
    /// The table has the following columns:
    ///
    /// <code>
    /// | Source Path | Destination Path | Status | Parsed Date | Parser Name |
    /// </code>
    /// </remarks>
    ICollection<KeyValuePair<string, MarkdownTable>> DescribeWrites(ICollection<FileOperationResult> fileOperationResults);
}