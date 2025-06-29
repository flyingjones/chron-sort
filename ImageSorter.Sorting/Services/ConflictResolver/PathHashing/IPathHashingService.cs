namespace ImageSorter.Sorting.Services.ConflictResolver.PathHashing;

public interface IPathHashingService
{
    /// <summary>
    /// Hashes a path and returns a string of length <paramref name="length"/> consisting of only lower case letters which
    /// represent the hash
    /// </summary>
    /// <remarks>
    /// Respects file system case sensitivity: If the file system is not case-sensitive, it will lower case the path
    /// before hashing
    /// </remarks>
    string HashPath(string path, int length);
}