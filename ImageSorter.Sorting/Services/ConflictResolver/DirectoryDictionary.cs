using ImageSorter.FileWrapper.Abstractions.Path;
using ImageSorter.Sorting.Abstractions.Model;
using ImageSorter.Sorting.Services.FilePath;

namespace ImageSorter.Sorting.Services.ConflictResolver;

public class DirectoryDictionary
{
    private readonly IPathWrapper _pathWrapper;
    private readonly IFilePathWrapperFactory _filePathWrapperFactory;

    public DirectoryDictionary(IPathWrapper pathWrapper, IFilePathWrapperFactory filePathWrapperFactory)
    {
        _pathWrapper = pathWrapper;
        _filePathWrapperFactory = filePathWrapperFactory;
    }

    private readonly Dictionary<string, DirectoryGroup> _underlyingDictionary = new Dictionary<string, DirectoryGroup>();

    public DirectoryGroup GetOrCreate(string directory)
    {
        if (_underlyingDictionary.TryGetValue(directory, out var result))
        {
            return result;
        }

        var newValue = new DirectoryGroup(_pathWrapper, _filePathWrapperFactory);
        _underlyingDictionary[directory] = newValue;

        return newValue;
    }

    public IEnumerable<string> Keys => _underlyingDictionary.Keys;

    public IEnumerable<SortedFilePath> AllFiles => _underlyingDictionary.Values.SelectMany(x => x.FilesInDirectory);
}