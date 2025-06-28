using System.Collections.ObjectModel;
using System.Text;

namespace ImageSorter.Sorting.SubServices.PathBuilder;

public class PathBuilder : IPathBuilder
{
    private readonly IReadOnlyList<string> _pathElements;
    private readonly string _destinationPath;
    
    public PathBuilder(PathBuilderOptions options)
    {
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

    public string BuildPath(DateTime dateTime)
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