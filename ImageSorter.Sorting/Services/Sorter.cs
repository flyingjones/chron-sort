using ImageSorter.FileWrapper.Abstractions.Path;
using ImageSorter.Sorting.Abstractions.Model;
using ImageSorter.Sorting.Abstractions.Services;
using ImageSorter.Sorting.SubServices.PathBuilder;

namespace ImageSorter.Sorting.Services;

public class Sorter : ISorter
{
    private readonly IPathBuilder _pathBuilder;
    private readonly IPathWrapper _pathWrapper;

    public Sorter(IPathBuilder pathBuilder, IPathWrapper pathWrapper)
    {
        _pathBuilder = pathBuilder;
        _pathWrapper = pathWrapper;
    }

    public ICollection<SortedFilePath> SortFiles(ICollection<FilePathWithParsedDate> filePaths)
    {
        var result = new SortedFilePath[filePaths.Count];

        var idx = 0;
        foreach (var filePath in filePaths)
        {
            var destinationPath = _pathWrapper.Combine(
                _pathBuilder.BuildPath(filePath.DateTime),
                _pathWrapper.GetFileName(filePath.FilePath) ?? throw new InvalidOperationException("file name not found"));
            
            result[idx] = new SortedFilePath
            {
                DateTime = filePath.DateTime,
                DestinationFilePath = destinationPath,
                SourceFilePath = filePath.FilePath
            };
            idx++;
        }
        
        return result;
    }
}