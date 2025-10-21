using ImageSorter.FileWrapper.Abstractions.Directory;
using ImageSorter.FileWrapper.Abstractions.FileHashing;
using ImageSorter.FileWrapper.Abstractions.Path;
using ImageSorter.Sorting.Abstractions.Model;
using ImageSorter.Sorting.Services.FilePath;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Sorting.Services.ConflictResolver;

public class ContentHashRenameConflictResolver : AbstractRenameBasedConflictResolver
{
    private const int HashNameLength = 6;
    private readonly IFileHashingService _fileHashingService;
    private readonly IPathWrapper _pathWrapper;

    public ContentHashRenameConflictResolver(
        IPathWrapper pathWrapper,
        IDirectoryWrapper directoryWrapper,
        ILogger<ContentHashRenameConflictResolver> logger,
        IFilePathWrapperFactory filePathWrapperFactory,
        IFileHashingService fileHashingService)
        : base(pathWrapper, directoryWrapper, logger, filePathWrapperFactory)
    {
        _pathWrapper = pathWrapper;
        _fileHashingService = fileHashingService;
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
        var fileName = _pathWrapper.GetFileNameWithoutExtension(originalPath);
        var fileExtension = _pathWrapper.GetExtension(originalPath);
        var hash = _fileHashingService.ComputeMd5HashAsString(originalPath, HashNameLength);
        return $"{fileName}_{hash}{fileExtension}";
    }
}