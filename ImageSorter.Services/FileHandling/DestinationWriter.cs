using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Services.FileHandling;

public class DestinationWriter : IDestinationWriter
{
    private readonly DestinationWriterOptions _options;
    private readonly ILogger<DestinationWriter> _logger;
    private readonly IDictionary<int, IDictionary<int, bool>> _filePaths;

    public DestinationWriter(DestinationWriterOptions options, ILogger<DestinationWriter> logger)
    {
        _filePaths = new ConcurrentDictionary<int, IDictionary<int, bool>>();
        _options = options;
        _logger = logger;
        Directory.CreateDirectory(options.DestinationPath);
    }

    public async Task CopyFile(string sourcePath, DateTime dateTime, CancellationToken cancellationToken)
    {
        var year = dateTime.Year;
        var month = dateTime.Month;
        var yearPath = $"{_options.DestinationPath}/{year:0000}";
        var monthPath = $"{yearPath}/{month:00}";
        if (!_filePaths.ContainsKey(year))
        {
            Directory.CreateDirectory(yearPath);
            _filePaths[year] = new ConcurrentDictionary<int, bool>();
        }

        if (!_filePaths[year].ContainsKey(month))
        {
            Directory.CreateDirectory(monthPath);
            _filePaths[year][month] = true;
        }

        var fileName = Path.GetFileName(sourcePath);
        var destinationPath = $"{monthPath}/{fileName}";
        try
        {
            if (File.Exists(destinationPath) && !_options.OverwriteExistingFiles)
            {
                return;
            }
            
            await using var sourceFileStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
            await using var destFileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);

            await sourceFileStream.CopyToAsync(destFileStream, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Could not write file {srcPath} to {destPath}, reason: {reason}", sourcePath, destinationPath, ex);
        }
    }

    public async Task CopyFiles(ICollection<WriteQueueItem> writeQueueItems, CancellationToken cancellationToken)
    {
        var yearGroups = writeQueueItems
            .Where(x => _options.From == null || x.DateTaken >= _options.From)
            .Where(x => _options.To == null || x.DateTaken <= _options.To)
            .OrderBy(x => x.DateTaken)
            .GroupBy(x => x.DateTaken.Year);
    
        _logger.LogInformation("Sorted {writeQueueCount} files in memory", writeQueueItems.Count);

        foreach (var yearGroup in yearGroups)
        {
            _logger.LogInformation("Writing year {year} ({fileCount} files)", yearGroup.Key, yearGroup.Count());
            foreach (var item in yearGroup)
            {
                await CopyFile(item.FilePath, item.DateTaken, cancellationToken);
            }
        }
    }
}