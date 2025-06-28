using ImageSorter.FileWrapper.Abstractions.Directory;
using ImageSorter.FileWrapper.Abstractions.Path;
using ImageSorter.Sorting.Abstractions.Model;
using ImageSorter.Sorting.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Sorting.Services.ConflictResolver;

public abstract class AbstractRenameBasedConflictResolver : IConflictResolver
{
    private const int MaxTries = 25;
    private readonly IPathWrapper _pathWrapper;
    private readonly IDirectoryWrapper _directoryWrapper;
    private readonly ILogger _logger;

    protected AbstractRenameBasedConflictResolver(
        IPathWrapper pathWrapper,
        IDirectoryWrapper directoryWrapper,
        ILogger logger)
    {
        _pathWrapper = pathWrapper;
        _directoryWrapper = directoryWrapper;
        _logger = logger;
    }
    
    public ICollection<SortedFilePath> ResolveConflicts(SortingConflictSummary sortingConflictSummary,
        HashSet<string> filesAtDestination)
    {
        // first group everything by directory ... renames must check for new collisions in their directory
        var directories = new DirectoryDictionary(_pathWrapper);

        // initialize data structure with non-conflicting files
        foreach (var group in sortingConflictSummary.NonConflictingFiles
                     .GroupBy(x => _directoryWrapper.GetParentDirectory(x.DestinationFilePath)))
        {
            if (group.Key == null)
            {
                _logger.LogWarning("Encountered directory null");
                continue;
            }

            var directoryGroup = directories.GetOrCreate(group.Key);
            directoryGroup.AddNonConflictingFiles(group.ToArray());
        }

        // then add existing files
        foreach (var group in filesAtDestination.GroupBy(x => _directoryWrapper.GetParentDirectory(x)))
        {
            if (group.Key == null)
            {
                _logger.LogWarning("Encountered directory null");
                continue;
            }

            var directoryGroup = directories.GetOrCreate(group.Key);
            directoryGroup.AddExistingFiles(group.ToArray());
        }

        // finally enumerate groups of conflicts by directory and rename them as needed
        foreach (var group in sortingConflictSummary.Conflicts
                     .GroupBy(x =>
                         _directoryWrapper.GetParentDirectory(x.ConflictingFiles.First().DestinationFilePath)))
        {
            if (group.Key == null)
            {
                _logger.LogWarning("Encountered directory null");
                continue;
            }

            var directoryGroup = directories.GetOrCreate(group.Key);
            // iterate rename for each file in conflict
            foreach (var conflict in group)
            {
                for (int i = 1; i <= MaxTries; i++)
                {
                    var proposedFileNames = GetProposedRenameNames(
                        conflict.ConflictingFiles
                            .Where(x => x.SortedFilePath != null)
                            .Select(x => x.SortedFilePath!),
                        i);

                    if (proposedFileNames.All(x => directoryGroup.FileNameAvailable(x.Value)))
                    {
                        var tmp = proposedFileNames.Select(x => new SortedFilePath
                        {
                            SourceFilePath = x.Key.SourceFilePath,
                            DestinationFilePath = _pathWrapper.Combine(group.Key, x.Value),
                            DateTime = x.Key.DateTime
                        }).ToArray();
                        directoryGroup.AddRenamedFiles(tmp);
                        break;
                    }
                }
            }
        }

        return directories.AllFiles.ToArray();
    }

    protected abstract ICollection<KeyValuePair<SortedFilePath, string>> GetProposedRenameNames(
        IEnumerable<SortedFilePath> filePaths, int depth);
}