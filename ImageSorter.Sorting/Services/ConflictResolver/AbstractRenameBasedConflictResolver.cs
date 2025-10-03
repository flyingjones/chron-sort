using ImageSorter.FileWrapper.Abstractions.Directory;
using ImageSorter.FileWrapper.Abstractions.Path;
using ImageSorter.Sorting.Abstractions.Model;
using ImageSorter.Sorting.Abstractions.Services;
using ImageSorter.Sorting.Services.FilePath;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Sorting.Services.ConflictResolver;

public abstract class AbstractRenameBasedConflictResolver : IConflictResolver
{
    private const int MaxTries = 25;
    private readonly IPathWrapper _pathWrapper;
    private readonly IDirectoryWrapper _directoryWrapper;
    private readonly ILogger _logger;
    private readonly IFilePathWrapperFactory _filePathWrapperFactory;

    protected AbstractRenameBasedConflictResolver(
        IPathWrapper pathWrapper,
        IDirectoryWrapper directoryWrapper,
        ILogger logger,
        IFilePathWrapperFactory filePathWrapperFactory)
    {
        _pathWrapper = pathWrapper;
        _directoryWrapper = directoryWrapper;
        _logger = logger;
        _filePathWrapperFactory = filePathWrapperFactory;
    }

    public ICollection<SortedFilePath> ResolveConflicts(
        SortingConflictSummary sortingConflictSummary,
        string[] filesAtDestination,
        out ICollection<SortedFilePath> discardedFiles)
    {
        var tmpDiscardedFiles = new List<SortedFilePath>();
        // first group everything by directory ... renames must check for new collisions in their directory
        var directories = new DirectoryDictionary(_pathWrapper, _filePathWrapperFactory);

        // initialize data structure with non-conflicting files
        foreach (var group in sortingConflictSummary.NonConflictingFiles
                     .GroupBy(x =>
                         _filePathWrapperFactory.GetNormalizedPath(
                             _directoryWrapper.GetParentDirectory(x.DestinationFilePath))))
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
        foreach (var group in filesAtDestination.GroupBy(x =>
                     _filePathWrapperFactory.GetNormalizedPath(_directoryWrapper.GetParentDirectory(x))))
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
                         _filePathWrapperFactory.GetNormalizedPath(
                             _directoryWrapper.GetParentDirectory(x.ConflictingFiles.First().DestinationFilePath))))
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
                ICollection<KeyValuePair<SortedFilePath, string>> proposedFileNames =
                    new List<KeyValuePair<SortedFilePath, string>>();
                for (int i = 1; i <= MaxTries; i++)
                {
                    proposedFileNames = GetProposedRenameNames(
                        conflict.ConflictingFiles
                            .Where(x => x.SortedFilePath != null)
                            .Select(x => x.SortedFilePath!),
                        i, out var stop);

                    if (stop)
                    {
                        i = MaxTries;
                    }

                    if (proposedFileNames.Select(x => x.Value).Distinct().Count() < proposedFileNames.Count)
                    {
                        // rename resulted in renaming all files the same way, try again!
                        continue;
                    }

                    if (proposedFileNames.All(x => directoryGroup.FileNameAvailable(x.Value)))
                    {
                        // we found a way to rename the files without any conflict, break and apply
                        break;
                    }
                }

                // add renamed files which can be added
                foreach (var proposedFileName in proposedFileNames)
                {
                    if (directoryGroup.FileNameAvailable(proposedFileName.Value))
                    {
                        directoryGroup.AddRenamedFile(
                            proposedFileName.Key with { DestinationFilePath = _pathWrapper.Combine(group.Key, proposedFileName.Value) });
                    }
                    else
                    {
                        tmpDiscardedFiles.Add(proposedFileName.Key);
                    }
                }
            }
        }

        discardedFiles = tmpDiscardedFiles;
        return directories.AllFiles.ToArray();
    }

    protected abstract ICollection<KeyValuePair<SortedFilePath, string>> GetProposedRenameNames(
        IEnumerable<SortedFilePath> filePaths, int depth, out bool stopRetries);
}