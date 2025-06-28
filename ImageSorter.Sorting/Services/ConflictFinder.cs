using ImageSorter.Sorting.Abstractions.Model;
using ImageSorter.Sorting.Abstractions.Services;

namespace ImageSorter.Sorting.Services;

public class ConflictFinder : IConflictFinder
{
    public SortingConflictSummary FindConflicts(ICollection<SortedFilePath> filePaths, ICollection<string> filesAtDestination)
    {
        var filesToProcess = filePaths
            .Select(x => new FilePathWrapper(x))
            .Concat(filesAtDestination.Select(filePath => new FilePathWrapper(filePath)))
            .ToArray();
        
        var groups = filesToProcess.GroupBy(x => x.DestinationFilePath).ToArray();

        var nonConflictingFiles = groups
            .Where(x => x.Count() == 1)
            .SelectMany(x => x)
            .Where(x => x.IsFromSource)
            .Select(x => x.SortedFilePath!)
            .ToArray();

        var conflicts = groups
            .Where(x => x.Count() > 1)
            .Select(conflictingFiles => new SortingConflict
            {
                ConflictingFiles = conflictingFiles.ToArray()
            })
            .ToArray();

        return new SortingConflictSummary
        {
            Conflicts = conflicts,
            NonConflictingFiles = nonConflictingFiles
        };
    }
}