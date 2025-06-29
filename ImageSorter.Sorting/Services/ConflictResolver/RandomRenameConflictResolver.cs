using ImageParser.Utils.RandomWrapper;
using ImageSorter.FileWrapper.Abstractions.Directory;
using ImageSorter.FileWrapper.Abstractions.Path;
using ImageSorter.Sorting.Abstractions.Model;
using ImageSorter.Sorting.Abstractions.Services;
using ImageSorter.Sorting.Services.FilePath;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Sorting.Services.ConflictResolver;

public class RandomRenameConflictResolver : AbstractRenameBasedConflictResolver
{
    private const int RandomNameLength = 5;
    private readonly IPathWrapper _pathWrapper;
    private readonly IDirectoryWrapper _directoryWrapper;
    private readonly ILogger<RandomRenameConflictResolver> _logger;
    private readonly IRandomStringGenerator _randomStringGenerator;

    public RandomRenameConflictResolver(
        IPathWrapper pathWrapper,
        IDirectoryWrapper directoryWrapper,
        ILogger<RandomRenameConflictResolver> logger, 
        IRandomStringGenerator randomStringGenerator, 
        IFilePathWrapperFactory filePathWrapperFactory) 
        : base(pathWrapper, directoryWrapper, logger, filePathWrapperFactory)
    {
        _pathWrapper = pathWrapper;
        _directoryWrapper = directoryWrapper;
        _logger = logger;
        _randomStringGenerator = randomStringGenerator;
    }

    protected override ICollection<KeyValuePair<SortedFilePath, string>> GetProposedRenameNames(IEnumerable<SortedFilePath> filePaths, int depth)
    {
        return filePaths.Select(filePath => new KeyValuePair<SortedFilePath, string>(
                filePath,
                BuildRandomFileName(filePath.SourceFilePath)))
            .ToArray();
    }

    private string BuildRandomFileName(string originalPath)
    {
        var fileName = _pathWrapper.GetFileName(originalPath)!;
        return $"{_randomStringGenerator.GenerateRandomString(RandomNameLength)}_{fileName}";
    }
}