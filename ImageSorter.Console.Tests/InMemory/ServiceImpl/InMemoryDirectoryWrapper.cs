using ImageSorter.FileWrapper.Abstractions.Directory;

namespace ImageSorter.Tests.InMemory.ServiceImpl;

public class InMemoryDirectoryWrapper : IDirectoryWrapper
{
    private readonly InMemoryFileSystem _inMemoryFileSystem;

    public InMemoryDirectoryWrapper(InMemoryFileSystem inMemoryFileSystem)
    {
        _inMemoryFileSystem = inMemoryFileSystem;
    }

    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        return _inMemoryFileSystem.GetFiles(path, searchPattern, searchOption);
    }

    public void CreateDirectory(string path)
    {
        _inMemoryFileSystem.CreateDirectory(path);
    }

    public void Delete(string path)
    {
        _inMemoryFileSystem.Delete(path);
    }

    public IEnumerable<string> EnumerateDirectories(string path)
    {
        return _inMemoryFileSystem.EnumerateDirectories(path);
    }

    public IEnumerable<string> EnumerateFileSystemEntries(string path)
    {
        return _inMemoryFileSystem.EnumerateFileSystemEntries(path);
    }

    public string? GetParentDirectory(string path)
    {
        return System.IO.Directory.GetParent(path)?.FullName;
    }
}