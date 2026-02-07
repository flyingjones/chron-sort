using System.Diagnostics.CodeAnalysis;
using ImageSorter.FileWrapper.Abstractions.File;
using ImageSorter.Tests.InMemory.Model;

namespace ImageSorter.Tests.InMemory.ServiceImpl;

public class InMemoryFileWrapper : IFileWrapper
{
    private readonly InMemoryFileSystem _inMemoryFileSystem;

    public InMemoryFileWrapper(InMemoryFileSystem inMemoryFileSystem)
    {
        _inMemoryFileSystem = inMemoryFileSystem;
    }
    
    public bool Exists([NotNullWhen(true)] string? path)
    {
        return _inMemoryFileSystem.FileExists(path);
    }

    public void Move(string sourceFileName, string destFileName, bool overwrite)
    {
        _inMemoryFileSystem.MoveFile(sourceFileName, destFileName, overwrite);
    }

    public long FileSize(string path)
    {
        return _inMemoryFileSystem.FileSize(path);
    }
}