using System.Collections.Concurrent;
using System.Text;
using ImageSorter.Services.FileWrapper;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Services.FileHandling;

public partial class DestinationWriter : IDestinationWriter
{
    private readonly DestinationWriterOptions _options;
    private readonly ILogger<DestinationWriter> _logger;
    private readonly IDictionary<int, IDictionary<int, bool>> _filePaths;
    private readonly IFileWrapper _fileWrapper;
    private readonly IDirectoryWrapper _directoryWrapper;
    private readonly IFileStreamService _fileStreamService;

    public DestinationWriter(DestinationWriterOptions options, ILogger<DestinationWriter> logger, IFileWrapper fileWrapper, IDirectoryWrapper directoryWrapper, IFileStreamService fileStreamService)
    {
        _filePaths = new ConcurrentDictionary<int, IDictionary<int, bool>>();
        _options = options;
        _logger = logger;
        _fileWrapper = fileWrapper;
        _directoryWrapper = directoryWrapper;
        _fileStreamService = fileStreamService;
        _directoryWrapper.CreateDirectory(options.DestinationPath);
    }

    public async Task CopyFile(string sourcePath, DateTime dateTime, CancellationToken cancellationToken)
    {
        var year = dateTime.Year;
        var month = dateTime.Month;
        var yearPath = $"{_options.DestinationPath}/{year:0000}";
        var monthPath = $"{yearPath}/{month:00}";
        if (!_filePaths.ContainsKey(year))
        {
            _directoryWrapper.CreateDirectory(yearPath);
            _filePaths[year] = new ConcurrentDictionary<int, bool>();
        }

        if (!_filePaths[year].ContainsKey(month))
        {
            _directoryWrapper.CreateDirectory(monthPath);
            _filePaths[year][month] = true;
        }

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
            _logger.LogTrace("Could not write file {srcPath} to {destPath}, reason: {reason}", sourcePath,
                destinationPath, ex);
        }
    }
    
    public void MoveFile(string sourcePath, DateTime dateTime)
    {
        var year = dateTime.Year;
        var month = dateTime.Month;
        var yearPath = $"{_options.DestinationPath}/{year:0000}";
        var monthPath = $"{yearPath}/{month:00}";
        if (!_filePaths.ContainsKey(year))
        {
            _directoryWrapper.CreateDirectory(yearPath);
            _filePaths[year] = new ConcurrentDictionary<int, bool>();
        }

        if (!_filePaths[year].ContainsKey(month))
        {
            _directoryWrapper.CreateDirectory(monthPath);
            _filePaths[year][month] = true;
        }

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
            _logger.LogTrace("Could not write file {srcPath} to {destPath}, reason: {reason}", sourcePath,
                destinationPath, ex);
        }
    }

    public async Task CopyFiles(IEnumerable<WriteQueueItem> writeQueueItems, CancellationToken cancellationToken)
    {
        var yearGroups = writeQueueItems
            .Where(x => _options.From == null || x.DateTaken >= _options.From)
            .Where(x => _options.To == null || x.DateTaken <= _options.To)
            .OrderBy(x => x.DateTaken)
            .GroupBy(x => x.DateTaken.Year)
            .ToList();

        LogStartupMessage(_logger, FormatSortSummary(yearGroups));

        var idx = 0;
        var count = yearGroups.SelectMany(x => x).Count();
        foreach (var yearGroup in yearGroups)
        {
            _logger.LogInformation("Writing year {year} ({fileCount} files)", yearGroup.Key, yearGroup.Count());
            foreach (var item in yearGroup)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
                
                await CopyFile(item.FilePath, item.DateTaken, cancellationToken);
                idx++;

                if (idx % _options.ProgressCount == 0)
                {
                    var progressString = (((double)idx / count) * 100).ToString("00");
                    _logger.LogInformation("Total Progress: {idx}/{count} ({progress}%)", idx, count, progressString);
                }
            }
        }
    }

    public void MoveFiles(IEnumerable<WriteQueueItem> writeQueueItems, CancellationToken cancellationToken)
    {
        var yearGroups = writeQueueItems
            .Where(x => _options.From == null || x.DateTaken >= _options.From)
            .Where(x => _options.To == null || x.DateTaken <= _options.To)
            .OrderBy(x => x.DateTaken)
            .GroupBy(x => x.DateTaken.Year)
            .ToList();

        LogStartupMessage(_logger, FormatSortSummary(yearGroups));

        var idx = 0;
        var count = yearGroups.SelectMany(x => x).Count();
        foreach (var yearGroup in yearGroups)
        {
            _logger.LogInformation("Moving year {year} ({fileCount} files)", yearGroup.Key, yearGroup.Count());
            foreach (var item in yearGroup)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
                
                MoveFile(item.FilePath, item.DateTaken);
                idx++;

                if (idx % _options.ProgressCount == 0)
                {
                    var progressString = (((double)idx / count) * 100).ToString("00");
                    _logger.LogInformation("Total Progress: {idx}/{count} ({progress}%)", idx, count, progressString);
                }
            }
        }
        
        DeleteEmptyDirs(_options.SourcePath);
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
                catch (UnauthorizedAccessException) { }
                catch (DirectoryNotFoundException) { }
            }
        }
        catch (UnauthorizedAccessException) { }
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

    [LoggerMessage(Level = LogLevel.Information, Message = "Sorting Summary {summary}")]
    private static partial void LogStartupMessage(ILogger logger, string summary);
}