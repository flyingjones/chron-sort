using System.Text;
using ImageSorter.Services.DateTimeWrapper;
using ImageSorter.Services.FileWrapper;
using ImageSorter.Services.ProgressLogger;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Services.FileHandling;

/// <inheritdoc cref="IDestinationWriter"/>
public partial class DestinationWriter : IDestinationWriter
{
    private readonly DestinationWriterOptions _options;
    private readonly ILogger<DestinationWriter> _logger;
    private readonly IFileWrapper _fileWrapper;
    private readonly IDirectoryWrapper _directoryWrapper;
    private readonly IFileStreamService _fileStreamService;
    private readonly IDateDirectory _dateDirectory;
    private readonly IProgressLogger<DestinationWriter> _progressLogger;
    private readonly IBufferedStreamWriterFactory _bufferedStreamWriterFactory;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DestinationWriter(
        DestinationWriterOptions options,
        ILogger<DestinationWriter> logger,
        IFileWrapper fileWrapper,
        IDirectoryWrapper directoryWrapper,
        IFileStreamService fileStreamService,
        IDateDirectory dateDirectory,
        IProgressLogger<DestinationWriter> progressLogger,
        IBufferedStreamWriterFactory bufferedStreamWriterFactory,
        IDateTimeProvider dateTimeProvider)
    {
        _options = options;
        _logger = logger;
        _fileWrapper = fileWrapper;
        _directoryWrapper = directoryWrapper;
        _fileStreamService = fileStreamService;
        _dateDirectory = dateDirectory;
        _progressLogger = progressLogger;
        _bufferedStreamWriterFactory = bufferedStreamWriterFactory;
        _dateTimeProvider = dateTimeProvider;
        _directoryWrapper.CreateDirectory(options.DestinationPath);
    }

    public async Task<FileOperationResult> CopyFile(string sourcePath, DateTime dateTime, CancellationToken cancellationToken)
    {
        var monthPath = _dateDirectory.CreatePathAndDirs(dateTime);
        var fileName = Path.GetFileName(sourcePath);
        var destinationPath = Path.GetFullPath($"{monthPath}/{fileName}");
        var result = new FileOperationResult
        {
            SourcePath = sourcePath,
            DestinationPath = destinationPath,
            Status = FileOperationResultStatus.Error,
            FileDate = dateTime
        };
        
        LogWriting("Copying", sourcePath, destinationPath);
        try
        {
            var fileExists = _fileWrapper.Exists(destinationPath);
            if (fileExists && !_options.OverwriteExistingFiles)
            {
                LogSkip();
                result.Status = FileOperationResultStatus.Skipped;
                return result;
            }

            if (fileExists) LogOverwrite();
            await _fileStreamService.CopyToAsync(sourcePath, destinationPath, cancellationToken);
            result.Status = fileExists ? FileOperationResultStatus.OverwriteSuccess : FileOperationResultStatus.Success;
        }
        catch (Exception ex)
        {
            LogError(ex, sourcePath, destinationPath);
        }

        return result;
    }

    public FileOperationResult MoveFile(string sourcePath, DateTime dateTime)
    {
        var monthPath = _dateDirectory.CreatePathAndDirs(dateTime);
        var fileName = Path.GetFileName(sourcePath);
        var destinationPath = Path.GetFullPath($"{monthPath}/{fileName}");
        var result = new FileOperationResult
        {
            SourcePath = sourcePath,
            DestinationPath = destinationPath,
            Status = FileOperationResultStatus.Error,
            FileDate = dateTime
        };
        
        LogWriting("Moving", sourcePath, destinationPath);
        // the file is now at the desired location -> return true
        if (sourcePath == destinationPath)
        {
            result.Status = FileOperationResultStatus.AlreadyInCorrectPlace;
            return result;
        }

        try
        {
            var fileExists = _fileWrapper.Exists(destinationPath);
            if (fileExists && !_options.OverwriteExistingFiles)
            {
                LogSkip();
                result.Status = FileOperationResultStatus.Skipped;
                return result;
            }
            
            if (fileExists) LogOverwrite();
            _fileWrapper.Move(sourcePath, destinationPath, _options.OverwriteExistingFiles);
            result.Status = fileExists ? FileOperationResultStatus.OverwriteSuccess : FileOperationResultStatus.Success;
        }
        catch (Exception ex)
        {
            LogError(ex, sourcePath, destinationPath);
        }

        return result;
    }

    /// <inheritdoc cref="IDestinationWriter.CopyFiles"/>
    public async Task CopyFiles(IEnumerable<WriteQueueItem> writeQueueItems, CancellationToken cancellationToken)
    {
        var yearGroups = OrderAndGroupWriteQueue(writeQueueItems);
        LogSummaryMessage(FormatSortSummary(yearGroups));

        var count = yearGroups.SelectMany(x => x).Count();

        GenerateDuplicateDescription(yearGroups.SelectMany(x => x));

        var copySummary = new List<FileOperationResult>();
        
        _progressLogger.LogStart("Copying {count} files (this may take a while)", count);
        
        var idx = 0;
        
        foreach (var yearGroup in yearGroups)
        {
            foreach (var item in yearGroup)
            {
                _progressLogger.LogProgress((double) idx / count);
                
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogError("Execution Canceled");
                    return;
                }

                var copyResult = await CopyFile(item.FilePath, item.DateTaken, cancellationToken);
                copyResult.ParserName = item.ParserName;
                copySummary.Add(copyResult);
                ++idx;
            }
        }
        
        _progressLogger.LogProgressFinished();
        
        _logger.LogInformation("Sorting Result Summary{Summary}", FormatSortResultSummary(copySummary));
        WriteSummaryFile(_options.DestinationPath, copySummary);
    }

    /// <inheritdoc cref="IDestinationWriter.MoveFiles"/>
    public void MoveFiles(IEnumerable<WriteQueueItem> writeQueueItems, CancellationToken cancellationToken)
    {
        var yearGroups = OrderAndGroupWriteQueue(writeQueueItems);
        LogSummaryMessage(FormatSortSummary(yearGroups));
        var count = yearGroups.SelectMany(x => x).Count();
        
        GenerateDuplicateDescription(yearGroups.SelectMany(x => x));
        
        var moveSummary = new List<FileOperationResult>();
        
        _progressLogger.LogStart("Moving {count} files (this may take a while)", count);
        
        var idx = 0;
        
        foreach (var yearGroup in yearGroups)
        {
            foreach (var item in yearGroup)
            {
                _progressLogger.LogProgress((double) idx / count);
                
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogError("Execution Canceled");
                    return;
                }

                var moveResult = MoveFile(item.FilePath, item.DateTaken);
                moveResult.ParserName = item.ParserName;
                ++idx;
                moveSummary.Add(moveResult);
            }
        }
        
        _progressLogger.LogProgressFinished();

        DeleteEmptyDirs(_options.SourcePath);
        _logger.LogInformation("Sorting Result Summary{Summary}", FormatSortResultSummary(moveSummary));
        WriteSummaryFile(_options.DestinationPath, moveSummary);
    }

    private void GenerateDuplicateDescription(IEnumerable<WriteQueueItem> writeQueueItems)
    {
        var duplicateCount = writeQueueItems
            .Select(item =>
            {
                var monthPath = _dateDirectory.CreatePathAndDirs(item.DateTaken);
                var fileName = Path.GetFileName(item.FilePath);
                return Path.GetFullPath($"{monthPath}/{fileName}");
            }).CountBy(x => x).Where(x => x.Value > 2).ToList();

        if (duplicateCount.Count > 0)
        {
            LogDuplicates(duplicateCount);
        }
    }

    private void LogDuplicates(ICollection<KeyValuePair<string, int>> duplicates)
    {
        foreach (var file in duplicates)
        {
            LogDuplicate(file.Key, file.Value);
        }

        var totalDuplicateCount = duplicates.Sum(x => x.Value);
        LogDuplicateCount(totalDuplicateCount);
    }

    /// <summary>
    /// Delete all empty subdirectories
    /// </summary>
    private void DeleteEmptyDirs(string path)
    {
        try
        {
            foreach (var d in _directoryWrapper.EnumerateDirectories(path))
            {
                DeleteEmptyDirs(d);
            }

            var entries = _directoryWrapper.EnumerateFileSystemEntries(path);

            if (!entries.Any())
            {
                try
                {
                    _directoryWrapper.Delete(path);
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (DirectoryNotFoundException)
                {
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private ICollection<IGrouping<int, WriteQueueItem>> OrderAndGroupWriteQueue(
        IEnumerable<WriteQueueItem> queue)
    {
        return queue.Where(x => _options.From == null || x.DateTaken >= _options.From)
            .Where(x => _options.To == null || x.DateTaken <= _options.To)
            .OrderBy(x => x.DateTaken)
            .GroupBy(x => x.DateTaken.Year)
            .ToList();
    }

    private void WriteSummaryFile(string destinationPath, ICollection<FileOperationResult> results)
    {
        // TODO move to own class and add flag to generate the summary file
        var summaryFileName = $"sort_result_summary_{_dateTimeProvider.Now():yyyy-MM-dd_hh_mm_ss}.txt";
        var summaryFilePath = $"{destinationPath}/{summaryFileName}";
        using var textWriter = _bufferedStreamWriterFactory.CreateStreamWriter(summaryFilePath, FileMode.CreateNew);

        foreach (var result in results)
        {
            textWriter.WriteLine(result.ToString());
        }
    }

    private static string FormatSortResultSummary(ICollection<FileOperationResult> operationResults)
    {
        var duplicatesFromSourceCount = operationResults
            .CountBy(x => x.DestinationPath)
            .Where(x => x.Value > 2)
            // one file will be moved
            .Sum(x => x.Value - 1);

        var countByStatus = operationResults
            .CountBy(x => x.Status)
            .ToDictionary();

        foreach (var possibleStatus in Enum.GetValues<FileOperationResultStatus>())
        {
            countByStatus.TryAdd(possibleStatus, 0);
        }

        var stringBuilder = new StringBuilder();

        stringBuilder.Append(Environment.NewLine);
        
        if (countByStatus[FileOperationResultStatus.AlreadyInCorrectPlace] > 0)
        {
            stringBuilder.Append($"[Already Sorted       ]: {countByStatus[FileOperationResultStatus.AlreadyInCorrectPlace],6:#####0}");
            stringBuilder.Append(Environment.NewLine);
        }
        
        stringBuilder.Append($"[Success              ]: {countByStatus[FileOperationResultStatus.Success],6:#####0}");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append($"[Success (Overwritten)]: {countByStatus[FileOperationResultStatus.OverwriteSuccess],6:#####0}");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append($"[Skipped              ]: {countByStatus[FileOperationResultStatus.Skipped],6:#####0} (duplicates in source: {duplicatesFromSourceCount})");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append($"[Error                ]: {countByStatus[FileOperationResultStatus.Error],6:#####0}");
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append($"[Total                ]: {operationResults.Count,6:#####0}");

        return stringBuilder.ToString();
    }

    private static string FormatSortSummary(ICollection<IGrouping<int, WriteQueueItem>> yearGroups)
    {
        var stringBuilder = new StringBuilder();

        var totalFileCount = yearGroups.SelectMany(x => x).Count();
        var width = totalFileCount.ToString().Length;

        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append(new string('-', width + 7));

        foreach (var yearGroup in yearGroups)
        {
            var countString = yearGroup.Count().ToString().PadLeft(width);
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append($"{yearGroup.Key} : {countString}");
        }

        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append($"Total: {totalFileCount}");

        return stringBuilder.ToString();
    }
    
    [LoggerMessage(Level = LogLevel.Information, Message = "Sorting with the current configuration will result in {count} files less in the destination because of duplicates in the source")]
    private partial void LogDuplicateCount(int count);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Target File {targetPath} is found in the source {count} times")]
    private partial void LogDuplicate(string targetPath, int count);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Skipping file since it already exists at the destination")]
    private partial void LogSkip();
    
    [LoggerMessage(Level = LogLevel.Trace, Message = "Overwriting existing file")]
    private partial void LogOverwrite();

    [LoggerMessage(Level = LogLevel.Trace, Message = "{operation} {sourcePath} to {destinationPath}")]
    private partial void LogWriting(string operation, string sourcePath, string destinationPath);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Could not write file {sourcePath} to {destinationPath}")]
    private partial void LogError(Exception exception, string sourcePath, string destinationPath);

    [LoggerMessage(Level = LogLevel.Information, Message = "Sorting Summary {summary}")]
    private partial void LogSummaryMessage(string summary);
}