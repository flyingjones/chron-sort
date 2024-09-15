using System.Collections.ObjectModel;
using System.Text;
using ImageSorter.Services.FileWrapper;

namespace ImageSorter.Services.FileHandling;

public class ConfigurableDateDirectory : IDateDirectory
{
    private readonly IReadOnlyList<string> _pathElements;
    private readonly HashSet<string> _createdPaths = new HashSet<string>();
    private readonly string _destinationPath;
    private readonly IDirectoryWrapper _directoryWrapper;
    private readonly bool _isDryRun;

    public ConfigurableDateDirectory(DateDirectoryOptions options, IDirectoryWrapper directoryWrapper)
    {
        _directoryWrapper = directoryWrapper;
        _isDryRun = options.DryRun;
        if (string.IsNullOrWhiteSpace(options.Format))
        {
            _pathElements = ReadOnlyCollection<string>.Empty;
        }
        else
        {
            _pathElements = new ReadOnlyCollection<string>(options.Format.Split("/"));
        }

        _destinationPath = options.DestinationPath;
    }

    public string CreatePathAndDirs(DateTime dateTime)
    {
        var path = BuildPath(dateTime);

        if (!_isDryRun && _createdPaths.Add(path))
        {
            _directoryWrapper.CreateDirectory(path);
        }

        return path;
    }

    private string BuildPath(DateTime dateTime)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.Append(_destinationPath);

        foreach (var pathElement in _pathElements)
        {
            var dir = dateTime.ToString(pathElement);
            stringBuilder.Append($"/{dir}");
        }

        return Path.GetFullPath(stringBuilder.ToString());
    }
}