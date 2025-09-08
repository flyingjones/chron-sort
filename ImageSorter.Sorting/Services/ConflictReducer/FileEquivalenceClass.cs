using ImageSorter.Sorting.Abstractions.Model;

namespace ImageSorter.Sorting.Services.ConflictReducer;

/// <summary>
/// Represents an <see href="https://en.wikipedia.org/wiki/Equivalence_class">equivalence class</see> of files in regard
/// to an equivalence relation.
/// </summary>
/// <remarks>
/// This means that all files in this class are pairwise equivalent according to the given equivalence relation.
/// This means that to check if another file is part of this class, we only need to check with one representative.
/// <br/>
/// Note that that relation doesn't need to be strict equality!
/// </remarks>
public class FileEquivalenceClass
{
    public FileEquivalenceClass(FilePathWrapper sortedFilePath)
    {
        Paths = new List<FilePathWrapper>();
        Paths.Add(sortedFilePath);
    }
    
    /// <summary>
    /// Get one file of the class, which represents the whole class.
    /// </summary>
    public string RepresentativePath => Paths.First().SourceFilePath;
    
    /// <summary>
    /// Get all files in the class
    /// </summary>
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