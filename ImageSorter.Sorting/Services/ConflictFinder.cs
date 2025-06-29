using ImageSorter.Sorting.Abstractions.Model;
using ImageSorter.Sorting.Abstractions.Services;
using ImageSorter.Sorting.Services.FilePath;

namespace ImageSorter.Sorting.Services;

public class ConflictFinder : IConflictFinder
{
    private readonly IFilePathWrapperFactory _filePathWrapperFactory;

    public ConflictFinder(IFilePathWrapperFactory filePathWrapperFactory)
    {
        _filePathWrapperFactory = filePathWrapperFactory;
    }

    public SortingConflictSummary FindConflicts(ICollection<SortedFilePath> filePaths, ICollection<string> filesAtDestination)
    {
        var filesToProcess = filePaths
            .Select(x => _filePathWrapperFactory.Create(x))
            .Concat(filesAtDestination.Select(filePath => _filePathWrapperFactory.Create(filePath)))
            .ToArray();
        
        var groups = filesToProcess.GroupBy(x => x.NormalizedDestinationPath).ToArray();

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