using System.Text;
using ImageSorter.Services.FileWrapper;
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

    public DestinationWriter(DestinationWriterOptions options, ILogger<DestinationWriter> logger,
        IFileWrapper fileWrapper, IDirectoryWrapper directoryWrapper, IFileStreamService fileStreamService,
        IDateDirectory dateDirectory)
    {
        _options = options;
        _logger = logger;
        _fileWrapper = fileWrapper;
        _directoryWrapper = directoryWrapper;
        _fileStreamService = fileStreamService;
        _dateDirectory = dateDirectory;
        _directoryWrapper.CreateDirectory(options.DestinationPath);
    }

    public async Task CopyFile(string sourcePath, DateTime dateTime, CancellationToken cancellationToken)
    {
        var monthPath = _dateDirectory.CreatePathAndDirs(dateTime);
        var fileName = Path.GetFileName(sourcePath);
        var destinationPath = $"{monthPath}/{fileName}";
        try
        {
            if (_fileWrapper.Exists(destinationPath) && !_options.OverwriteExistingFiles)
            {
                return;
            }

            await _fileStreamService.CopyToAsync(sourcePath, destinationPath, cancellationToken);
        }
        catch (Exception ex)
        {
            LogError(_logger, sourcePath, destinationPath, ex);
        }
    }

    public void MoveFile(string sourcePath, DateTime dateTime)
    {
        var monthPath = _dateDirectory.CreatePathAndDirs(dateTime);
        var fileName = Path.GetFileName(sourcePath);
        var destinationPath = $"{monthPath}/{fileName}";

        if (sourcePath == destinationPath) return;

        try
        {
            if (_fileWrapper.Exists(destinationPath) && !_options.OverwriteExistingFiles)
            {
                return;
            }

            _fileWrapper.Move(sourcePath, destinationPath, _options.OverwriteExistingFiles);
        }
        catch (Exception ex)
        {
            LogError(_logger, sourcePath, destinationPath, ex);
        }
    }

    /// <inheritdoc cref="IDestinationWriter.CopyFiles"/>
    public async Task CopyFiles(IEnumerable<WriteQueueItem> writeQueueItems, CancellationToken cancellationToken)
    {
        var yearGroups = OrderAndGroupWriteQueue(writeQueueItems);
        LogSummaryMessage(_logger, FormatSortSummary(yearGroups));

        var idx = 0;
        var count = yearGroups.SelectMany(x => x).Count();
        foreach (var yearGroup in yearGroups)
        {
            LogYearMessage(_logger, yearGroup.Key, yearGroup.Count());
            foreach (var item in yearGroup)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogError("Execution Canceled");
                    return;
                }

                await CopyFile(item.FilePath, item.DateTaken, cancellationToken);
                LogProgressIfRequired(++idx, count);
            }
        }
    }

    /// <inheritdoc cref="IDestinationWriter.MoveFiles"/>
    public void MoveFiles(IEnumerable<WriteQueueItem> writeQueueItems, CancellationToken cancellationToken)
    {
        var yearGroups = OrderAndGroupWriteQueue(writeQueueItems);

        LogSummaryMessage(_logger, FormatSortSummary(yearGroups));

        var idx = 0;
        var count = yearGroups.SelectMany(x => x).Count();
        foreach (var yearGroup in yearGroups)
        {
            LogYearMessage(_logger, yearGroup.Key, yearGroup.Count());
            foreach (var item in yearGroup)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogError("Execution Canceled");
                    return;
                }

                MoveFile(item.FilePath, item.DateTaken);
                LogProgressIfRequired(++idx, count);
            }
        }

        DeleteEmptyDirs(_options.SourcePath);
    }

    private void LogProgressIfRequired(int index, int totalCount)
    {
        if (index % _options.ProgressCount == 0)
        {
            var progressString = (((double)index / totalCount) * 100).ToString("00");
            LogProgress(_logger, index, totalCount, progressString);
        }
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

    [LoggerMessage(Level = LogLevel.Trace,
        Message = "Could not write file {sourcePath} to {destinationPath}, reason: {exception}")]
    private static partial void LogError(ILogger logger, string sourcePath, string destinationPath, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Total Progress: {index}/{totalCount} ({progressString}%)")]
    private static partial void LogProgress(ILogger logger, int index, int totalCount, string progressString);

    [LoggerMessage(Level = LogLevel.Information, Message = "Processing year {year} ({count} files)")]
    private static partial void LogYearMessage(ILogger logger, int year, int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "Sorting Summary {summary}")]
    private static partial void LogSummaryMessage(ILogger logger, string summary);
}