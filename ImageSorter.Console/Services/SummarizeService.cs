using ImageSorter.DateParsing.Abstractions.Model.MetaData;
using ImageSorter.Markdown.Abstractions.Model;
using ImageSorter.Markdown.Helper;
using ImageSorter.Services.FileHandling;

namespace ImageSorter.Services;

public class SummarizeService : ISummarizeService
{
    private readonly IDateDirectory _dateDirectory;

    public SummarizeService(IDateDirectory dateDirectory)
    {
        _dateDirectory = dateDirectory;
    }

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

    /// <inheritdoc cref="ISummarizeService.SummarizeConflicts"/>
    public MarkdownTable SummarizeConflicts(ICollection<ParsedFileResult> writeQueue)
    {
        var builder = new MarkdownTableBuilder();

        // header row
        builder.AddRow(
            "Year",
            "Total",
            "Conflicts",
            "Expected Skips",
            "Expected Count at Destination");

        // row for each year
        var totalConflicts = 0;
        var totalSkips = 0;
        var expectedTotalCount = 0;
        foreach (var yearCount in writeQueue.CountBy(x => x.DateTaken.Year).OrderBy(x => x.Key))
        {
            var totalCount = writeQueue
                .Where(x => x.DateTaken.Year == yearCount.Key)
                .CountBy(x => Path.Combine(_dateDirectory.BuildPath(x.DateTaken), Path.GetFileName(x.FilePath)))
                .ToArray();
            var conflictCount = totalCount
                .Where(x => x.Value > 1)
                .ToArray();
            var skippedFileCount = conflictCount
                .Select(x => x.Value - 1)
                .Sum();

            var conflictCountNumber = conflictCount.Sum(x => x.Value);
            totalConflicts += conflictCountNumber;
            totalSkips += skippedFileCount;
            expectedTotalCount += totalCount.Length;
            builder.AddRow(
                $"{yearCount.Key}",
                $"{yearCount.Value.ToString()}",
                $"{conflictCountNumber}",
                $"{skippedFileCount}",
                totalCount.Length.ToString());
        }

        // summary row
        builder.AddRow("*", writeQueue.Count.ToString(), totalConflicts.ToString(), totalSkips.ToString(),
            expectedTotalCount.ToString());

        var table = builder.Build();
        table.SetTextAlignment(HorizontalTextAlignment.Right);
        return table;
    }

    /// <inheritdoc cref="ISummarizeService.DescribeConflicts"/>
    public ICollection<KeyValuePair<string, MarkdownTable>> DescribeConflicts(ICollection<ParsedFileResult> writeQueue)
    {
        return writeQueue
            .OrderBy(x => x.DateTaken)
            // the group key is the destination file path of the sorting
            .GroupBy(x => Path.Combine(_dateDirectory.BuildPath(x.DateTaken), Path.GetFileName(x.FilePath)))
            .Where(x => x.Count() > 1)
            .Select(conflict =>
            {
                var tableBuilder = new MarkdownTableBuilder();

                // header row for each  conflict
                tableBuilder.AddRow("Source Path", "Parsed Date", "Parser Name");

                // body rows for each conflict
                foreach (var item in conflict)
                {
                    tableBuilder.AddRow(item.FilePath, item.DateTaken.ToString("s"), item.ParserName);
                }

                return new KeyValuePair<string, MarkdownTable>(conflict.Key, tableBuilder.Build());
            })
            .ToList();
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
    public MarkdownTable SummarizeWriteResults(ICollection<FileOperationResult> fileOperationResults)
    {
        var builder = new MarkdownTableBuilder();

        builder.AddRow("Year", "Total", "Error", "Skipped", "Already Sorted", "Success", "Success (Overwritten)");

        var groupedResults = fileOperationResults
            .GroupBy(x => x.FileDate.Year)
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
        
        builder.AddRow("*",
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