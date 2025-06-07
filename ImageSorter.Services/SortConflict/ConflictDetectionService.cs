using System.Collections.Immutable;
using System.Reflection.Metadata.Ecma335;
using ImageSorter.FileHandling.File;
using ImageSorter.Services.FileHandling;

namespace ImageSorter.Services.SortConflict;

public class ConflictDetectionService : IConflictDetectionService
{
    private readonly ConflictServiceOptions _serviceOptions;
    private readonly IDateDirectory _dateDirectory;
    private readonly IFileWrapper _fileWrapper;

    public ConflictDetectionService(
        ConflictServiceOptions serviceOptions,
        IDateDirectory dateDirectory,
        IFileWrapper fileWrapper)
    {
        _serviceOptions = serviceOptions;
        _dateDirectory = dateDirectory;
        _fileWrapper = fileWrapper;
    }


    public async Task<ConflictDetectionResult> FindConflicts(ICollection<WriteQueueItem> writeQueueItems)
    {
        // find raw conflicts
        var groups = writeQueueItems
            .OrderBy(x => x.DateTaken)
            // the group key is the destination file path of the sorting
            .GroupBy(x => Path.Combine(_dateDirectory.BuildPath(x.DateTaken), Path.GetFileName(x.FilePath)))
            .ToList();

        var nonConflicts = groups
            .Where(x => x.Count() == 1)
            .SelectMany(x => x)
            .ToList();

        var rawConflicts = groups
            .Where(x => x.Count() > 1);

        // apply equality checks to raw conflicts
        ICollection<Conflict> conflicts;
        switch (_serviceOptions.FileEqualsStrategy)
        {
            case FileEqualsStrategy.None:
            {
                conflicts = rawConflicts
                    .Select(group => new Conflict
                    {
                        ConflictingItems = group.ToList(),
                        SkippedEqualItems = Array.Empty<WriteQueueItem>()
                    }).ToList();
                break;
            }

            case FileEqualsStrategy.FileLength:
            {
                conflicts = rawConflicts
                    .Select(AnalyseConflictBasedOnFileLength)
                    .ToList();
                break;
            }

            case FileEqualsStrategy.FileContent:
            {
                throw new NotImplementedException();
                break;
            }

            default:
                throw new ArgumentOutOfRangeException(nameof(_serviceOptions.FileEqualsStrategy));
        }

        // return results
        return new ConflictDetectionResult
        {
            ConflictFreeItems = nonConflicts,
            Conflicts = conflicts
        };
    }

    private Conflict AnalyseConflictBasedOnFileLength(IEnumerable<WriteQueueItem> conflictingItems)
    {
        var lengthGroups = conflictingItems
            .GroupBy(x => _fileWrapper.FileLength(x.FilePath))
            .ToList();

        var conflict = new Conflict
        {
            ConflictingItems = new List<WriteQueueItem>(),
            SkippedEqualItems = new List<WriteQueueItem>()
        };

        foreach (var lengthGroup in lengthGroups)
        {
            var lengthGroupItems = lengthGroup.ToArray();
            for (var index = 0; index < lengthGroupItems.Length; index++)
            {
                if (index == 0)
                {
                    conflict.ConflictingItems.Add(lengthGroupItems[index]);
                }
                else
                {
                    conflict.SkippedEqualItems.Add(lengthGroupItems[index]);
                }
            }
        }

        return conflict;
    }
}