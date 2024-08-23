using System.Collections.Concurrent;
using ImageSorter.Services.FileWrapper;

namespace ImageSorter.Services.FileHandling;

/// <inheritdoc cref="IDateDirectory"/>
public class DateDirectory : IDateDirectory
{
    private readonly IDirectoryWrapper _directoryWrapper;
    private readonly DestinationWriterOptions _options;
    private readonly IDictionary<int, IDictionary<int, bool>> _filePaths =
        new ConcurrentDictionary<int, IDictionary<int, bool>>();

    public DateDirectory(IDirectoryWrapper directoryWrapper, DestinationWriterOptions options)
    {
        _directoryWrapper = directoryWrapper;
        _options = options;
    }

    /// <inheritdoc cref="IDateDirectory.CreatePathAndDirs"/>
    public string CreatePathAndDirs(DateTime dateTime)
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

        return monthPath;
    }
}