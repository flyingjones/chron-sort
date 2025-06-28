using ImageSorter.Sorting.Abstractions.Model;

namespace ImageSorter.Sorting.Services.ConflictReducer;

public class FileEqualityClass
{
    public FileEqualityClass(FilePathWrapper sortedFilePath)
    {
        Paths = new List<FilePathWrapper>();
        Paths.Add(sortedFilePath);
    }
    
    public string RepresentativePath => Paths.First().SourceFilePath;
    
    public ICollection<FilePathWrapper> Paths { get; set; }

    /// <summary>
    /// Whether the class is already present in the destination directory
    /// </summary>
    public bool IsRepresentedAtDestination => Paths.Any(x => !x.IsFromSource);

    public bool RepresentsPath(FilePathWrapper sortedFilePath)
    {
        return Paths.Contains(sortedFilePath);
    }
}