using ImageSorter.FileWrapper.Abstractions.Path;
using ImageSorter.Sorting.Abstractions.Model;

namespace ImageSorter.Sorting.Services.ConflictResolver;

public class DirectoryGroup
{
    private readonly IPathWrapper _pathWrapper;
    
    private readonly List<SortedFilePath> _nonConflictingFiles = new List<SortedFilePath>();
    private readonly List<string> _existingFiles = new List<string>();
    private readonly List<SortedFilePath> _renamedFiles = new List<SortedFilePath>();
    private readonly HashSet<string> _usedFileNames = new HashSet<string>();

    public DirectoryGroup(IPathWrapper pathWrapper)
    {
        _pathWrapper = pathWrapper;
    }

    public void AddNonConflictingFiles(ICollection<SortedFilePath> nonConflictingFiles)
    {
        _nonConflictingFiles.AddRange(nonConflictingFiles);
        var fileNames = nonConflictingFiles
            .Select(x => _pathWrapper.GetFileName(x.DestinationFilePath))
            .Where(x => x != null).Cast<string>();
        foreach (var fileName in fileNames)
        {
            _usedFileNames.Add(fileName);
        }
    }

    public void AddExistingFiles(ICollection<string> existingFiles)
    {
        _existingFiles.AddRange(existingFiles);
        var fileNames = existingFiles.Select(_pathWrapper.GetFileName).Where(x => x != null).Cast<string>();
        foreach (var fileName in fileNames)
        {
            _usedFileNames.Add(fileName);
        }
    }

    public bool FileNameAvailable(string filePath)
    {
        var fileName = _pathWrapper.GetFileName(filePath);
        if (fileName == null)
            return false;
        return !_usedFileNames.Contains(fileName);
    }

    public void AddRenamedFiles(ICollection<SortedFilePath> renamedFiles)
    {
        _renamedFiles.AddRange(renamedFiles);
        var fileNames = renamedFiles
            .Select(x => _pathWrapper.GetFileName(x.DestinationFilePath))
            .Where(x => x != null).Cast<string>();
        foreach (var fileName in fileNames)
        {
            _usedFileNames.Add(fileName);
        }
    }

    public ICollection<SortedFilePath> FilesInDirectory => _nonConflictingFiles.Concat(_renamedFiles).ToArray();
}