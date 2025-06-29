using ImageSorter.FileWrapper.Abstractions.Directory;
using ImageSorter.FileWrapper.Abstractions.Path;
using ImageSorter.Sorting.Abstractions.Model;
using ImageSorter.Sorting.Services.ConflictResolver.PathHashing;
using ImageSorter.Sorting.Services.FilePath;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Sorting.Services.ConflictResolver;

public class HashRenameConflictResolver : AbstractRenameBasedConflictResolver
{
    private const int HashNameLength = 6;
    private readonly IPathHashingService _pathHashingService;
    private readonly IPathWrapper _pathWrapper;

    public HashRenameConflictResolver(
        IPathWrapper pathWrapper,
        IDirectoryWrapper directoryWrapper,
        ILogger<HashRenameConflictResolver> logger,
        IFilePathWrapperFactory filePathWrapperFactory,
        IPathHashingService pathHashingService) 
        : base(pathWrapper, directoryWrapper, logger, filePathWrapperFactory)
    {
        _pathWrapper = pathWrapper;
        _pathHashingService = pathHashingService;
    }

    protected override ICollection<KeyValuePair<SortedFilePath, string>> GetProposedRenameNames(
        IEnumerable<SortedFilePath> filePaths, 
        int depth,
        out bool stopRetries)
    {
        // same hash means probably from the same source so stop retrying
        stopRetries = true;
        return filePaths.Select(filePath => new KeyValuePair<SortedFilePath, string>(
                filePath,
                BuildHashBasedFileName(filePath.SourceFilePath)))
            .ToArray();
    }

    private string BuildHashBasedFileName(string originalPath)
    {
        var fileName = _pathWrapper.GetFileName(originalPath)!;
        var hash = _pathHashingService.HashPath(originalPath, HashNameLength);
        return $"{hash}_{fileName}";
    }
}