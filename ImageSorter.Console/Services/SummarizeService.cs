using ImageSorter.Markdown.Abstractions.Model;
using ImageSorter.Markdown.Helper;
using ImageSorter.ResultWriting.Abstractions.Model;
using ImageSorter.Services.FileHandling;
using ImageSorter.Sorting.Abstractions.Model;
using FileOperationResultStatus = ImageSorter.ResultWriting.Abstractions.Model.FileOperationResultStatus;

namespace ImageSorter.Services;

public class SummarizeService : ISummarizeService
{
    /// <inheritdoc cref="ISummarizeService.SummarizeFileScan"/>
    public MarkdownTable SummarizeFileScan(string[] filesToProcess, string[] allFiles)
    {
        var builder = new MarkdownTableBuilder();
        
        // header row
        builder.AddRow("File Ending", "Used Count", "Ignored Count");

        var allFileEndingsCount = allFiles
            .Select(path => path.Split(".")[^1])
            .CountBy(x => x)
            .OrderByDescending(x => x.Value)
            .ToArray();

        var usedFileEndingsCount = filesToProcess
            .Select(path => path.Split(".")[^1])
            .CountBy(x => x)
            .ToDictionary();

        // row for each file ending
        foreach (var fileEnding in allFileEndingsCount)
        {
            var usedFilesCount = usedFileEndingsCount.GetValueOrDefault(fileEnding.Key, 0);
            builder.AddRow(fileEnding.Key, $"{usedFilesCount}", $"{(fileEnding.Value - usedFilesCount)}");
        }
        
        // summary row (all file endings)
        builder.AddRow("*", filesToProcess.Length.ToString(), (allFiles.Length - filesToProcess.Length).ToString());

        var table = builder.Build();
        table.SetTextAlignment(HorizontalTextAlignment.Right);
        return table;
    }

    /// <inheritdoc />
    public MarkdownTable SummarizeConflicts(ReducedSortingConflictSummary reducedSortingConflicts)
    {
        var builder = new MarkdownTableBuilder();

        // header row
        builder.AddRow(
            "Year",
            "Total",
            "Non Conflicting",
            "Conflicting",
            "Present at Destination",
            "Remaining Conflicting after Reduction",
            "Discarded",
            "Remaining Total");

        // body
        var yearNonConflictGroups = reducedSortingConflicts.NonConflictingFiles
            .GroupBy(x => x.DateTime.Year)
            .ToDictionary(x => x.First().DateTime.Year, x => x.ToArray());
            
        var yearConflictGroups = reducedSortingConflicts.ReducedSortingConflicts
            .GroupBy(x => x.DateTime.Year)
            .ToDictionary(x => x.First().DateTime.Year, x => x.ToArray());

        var years = yearNonConflictGroups.Keys.Concat(yearConflictGroups.Keys).Distinct().Order().ToArray();

        var totalTotalCount = 0;
        var totalRemainingConflicting = 0;
        var totalDiscarded = 0;
        var totalRemaining = 0;
        var totalPresentAtDestination = 0;
        var totalNonConflicting = 0;
        var totalConflicting = 0;
        foreach (var year in years)
        {
            SortedFilePath[] nonConflicting = [];
            if (yearNonConflictGroups.TryGetValue(year, out var tmp1))
                nonConflicting = tmp1;
            totalNonConflicting += nonConflicting.Length;

            ReducedSortingConflict[] reducedConflicts = [];
            if (yearConflictGroups.TryGetValue(year, out var tmp2))
                reducedConflicts = tmp2;
            
            var conflictingFileCount =
                reducedConflicts.Sum(x =>
                    x.ConflictingFiles.Count(y => y.IsFromSource));
            totalConflicting += conflictingFileCount;

            var totalFileAmount = nonConflicting.Length + conflictingFileCount;
            totalTotalCount += totalFileAmount;

            var remainingConflicting = reducedConflicts
                .Where(x => x.ChosenFiles.Count > 1)
                .Sum(x => x.ChosenFiles.Count);
            totalRemainingConflicting += remainingConflicting;

            var discardedFileCount =
                reducedConflicts.Sum(x => x.DiscardedFiles.Count);
            totalDiscarded += discardedFileCount;

            var remainingFileCount =
                reducedConflicts.Sum(x => x.ChosenFiles.Count) +
                nonConflicting.Length;
            totalRemaining += remainingFileCount;

            var presentAtDestinationCount = reducedConflicts
                .Where(x => x.ChosenFiles.Count == 0)
                .Sum(x => x.ConflictingFiles.Where(y => y.IsFromSource).Count());
            totalPresentAtDestination += presentAtDestinationCount;

            builder.AddRow(
                year.ToString(),
                totalFileAmount.ToString(),
                nonConflicting.Length.ToString(),
                conflictingFileCount.ToString(),
                presentAtDestinationCount.ToString(),
                remainingConflicting.ToString(),
                discardedFileCount.ToString(),
                remainingFileCount.ToString()
            );
        }
        
        builder.AddRow(
            "****",
            totalTotalCount.ToString(),
            totalNonConflicting.ToString(),
            totalConflicting.ToString(),
            totalPresentAtDestination.ToString(),
            totalRemainingConflicting.ToString(),
            totalDiscarded.ToString(),
            totalRemaining.ToString()
        );

        var table = builder.Build();
        table.SetTextAlignment(HorizontalTextAlignment.Right);
        return table;
    }

    /// <inheritdoc cref="ISummarizeService.DescribeWrites"/>
    public ICollection<KeyValuePair<string, MarkdownTable>> DescribeWrites(ICollection<FileOperationResult> fileOperationResults)
    {
        return fileOperationResults
            .OrderBy(x => x.DestinationPath)
            // the group key is the directory of the destination path
            .GroupBy(x => Path.GetDirectoryName(x.DestinationPath) ?? string.Empty)
            .Select(groupedResult =>
            {
                var tableBuilder = new MarkdownTableBuilder();
                // table header for each group
                tableBuilder.AddRow("Source Path", "Destination Path", "Status", "Parsed Date", "Parser Name");

                // table body
                foreach (var item in groupedResult)
                {
                    tableBuilder.AddRow(
                        item.SourcePath,
                        item.DestinationPath,
                        item.Status.ToString("G"),
                        item.FileDate.ToString("s"),
                        item.ParserName);
                }

                return new KeyValuePair<string, MarkdownTable>(groupedResult.Key, tableBuilder.Build());
            })
            .ToList();
    }

    /// <inheritdoc cref="ISummarizeService.SummarizeWriteResults"/>
    public MarkdownTable SummarizeWriteResults(ICollection<FileWriteResultDto> fileOperationResults)
    {
        var builder = new MarkdownTableBuilder();

        builder.AddRow("Year", "Total", "Error", "Skipped", "Already Sorted", "Success", "Success (Overwritten)");

        var groupedResults = fileOperationResults
            .GroupBy(x => x.DateTime.Year)
            .OrderBy(x => x.Key)
            .ToList();

        foreach (var groupedResult in groupedResults)
        {
            builder.AddRow($"{groupedResult.Key}",
                $"{groupedResult.Count()}",
                $"{groupedResult.Count(x => x.Status == FileOperationResultStatus.Error)}",
                $"{groupedResult.Count(x => x.Status == FileOperationResultStatus.Skipped)}",
                $"{groupedResult.Count(x => x.Status == FileOperationResultStatus.AlreadyInCorrectPlace)}",
                $"{groupedResult.Count(x => x.Status == FileOperationResultStatus.Success)}",
                $"{groupedResult.Count(x => x.Status == FileOperationResultStatus.OverwriteSuccess)}");
        }

        builder.AddRow("****",
            $"{fileOperationResults.Count}",
            $"{fileOperationResults.Count(x => x.Status == FileOperationResultStatus.Error)}",
            $"{fileOperationResults.Count(x => x.Status == FileOperationResultStatus.Skipped)}",
            $"{fileOperationResults.Count(x => x.Status == FileOperationResultStatus.AlreadyInCorrectPlace)}",
            $"{fileOperationResults.Count(x => x.Status == FileOperationResultStatus.Success)}",
            $"{fileOperationResults.Count(x => x.Status == FileOperationResultStatus.OverwriteSuccess)}");

        var table = builder.Build();
        table.SetTextAlignment(HorizontalTextAlignment.Right);
        return table;
    }
}