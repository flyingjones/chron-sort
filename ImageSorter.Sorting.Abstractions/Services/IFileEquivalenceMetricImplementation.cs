namespace ImageSorter.Sorting.Abstractions.Services;

/// <summary>
/// Defines an <see href="https://en.wikipedia.org/wiki/Equivalence_relation">equivalence relation</see> for files.
/// </summary>
public interface IFileEquivalenceMetricImplementation
{
    /// <summary>
    /// Check if two files are equivalent according to the implemented equivalence relation.
    /// </summary>
    /// <remarks>
    /// Note that the implementation needs to be en equivalence relation, which means:
    /// <br/>
    /// - it is reflexive: <c>path ~= path</c> is always true
    /// <br/>
    /// - it is symmetric: from <c>path1 ~= path2</c> follows that <c>path2 ~= path1</c>
    /// <br/>
    /// - it is transitive: from <c>path1 ~= path2</c> and <c>path2 ~= path3</c> follows that <c>path1 ~= path3</c>
    /// </remarks>
    Task<bool> FilesAreEquivalent(string path1, string path2, CancellationToken cancellationToken);
}