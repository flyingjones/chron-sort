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

    public void CopyFile(string sourcePath, DateTime dateTime)
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
            
            File.Copy(sourcePath, destinationPath, _options.OverwriteExistingFiles);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Could not write file {srcPath} to {destPath}, reason: {reason}", sourcePath, destinationPath, ex);
        }
    }
}