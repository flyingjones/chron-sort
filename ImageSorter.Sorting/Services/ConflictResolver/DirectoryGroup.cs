using ImageSorter.FileWrapper.Abstractions.Path;
using ImageSorter.Sorting.Abstractions.Model;
using ImageSorter.Sorting.Services.FilePath;

namespace ImageSorter.Sorting.Services.ConflictResolver;

public class DirectoryGroup
{
    private readonly IPathWrapper _pathWrapper;
    private readonly IFilePathWrapperFactory _filePathWrapperFactory;
    
    private readonly List<SortedFilePath> _nonConflictingFiles = new List<SortedFilePath>();
    private readonly List<string> _existingFiles = new List<string>();
    private readonly List<SortedFilePath> _renamedFiles = new List<SortedFilePath>();
    private readonly HashSet<string> _usedFileNames = new HashSet<string>();

    public DirectoryGroup(IPathWrapper pathWrapper, IFilePathWrapperFactory filePathWrapperFactory)
    {
        _pathWrapper = pathWrapper;
        _filePathWrapperFactory = filePathWrapperFactory;
    }

    public void AddNonConflictingFiles(ICollection<SortedFilePath> nonConflictingFiles)
    {
        _nonConflictingFiles.AddRange(nonConflictingFiles);
        var fileNames = nonConflictingFiles
            .Select(x => _pathWrapper.GetFileName(x.DestinationFilePath))
            .Where(x => x != null).Cast<string>();
        foreach (var fileName in fileNames)
        {
            _usedFileNames.Add(_filePathWrapperFactory.GetNormalizedPath(fileName));
        }
    }

    public void AddExistingFiles(ICollection<string> existingFiles)
    {
        _existingFiles.AddRange(existingFiles);
        var fileNames = existingFiles.Select(_pathWrapper.GetFileName).Where(x => x != null).Cast<string>();
        foreach (var fileName in fileNames)
        {
            _usedFileNames.Add(_filePathWrapperFactory.GetNormalizedPath(fileName));
        }
    }

    public bool FileNameAvailable(string filePath)
    {
        var fileName = _pathWrapper.GetFileName(filePath);
        if (fileName == null)
            return false;
        return !_usedFileNames.Contains(_filePathWrapperFactory.GetNormalizedPath(fileName));
    }

    public void AddRenamedFiles(ICollection<SortedFilePath> renamedFiles)
    {
        _renamedFiles.AddRange(renamedFiles);
        var fileNames = renamedFiles
            .Select(x => _pathWrapper.GetFileName(x.DestinationFilePath))
            .Where(x => x != null).Cast<string>();
        foreach (var fileName in fileNames)
        {
            _usedFileNames.Add(_filePathWrapperFactory.GetNormalizedPath(fileName));
        }
    }

    public void AddRenamedFile(SortedFilePath renamedFile)
    {
        _renamedFiles.Add(renamedFile);
        var fileName = _pathWrapper.GetFileName(renamedFile.DestinationFilePath)!;
        _usedFileNames.Add(_filePathWrapperFactory.GetNormalizedPath(fileName));
    }

    public ICollection<SortedFilePath> FilesInDirectory => _nonConflictingFiles.Concat(_renamedFiles).ToArray();
}