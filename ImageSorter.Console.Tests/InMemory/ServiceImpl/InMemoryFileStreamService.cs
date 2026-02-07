using ImageSorter.FileWrapper.Abstractions.FileStream;
using ImageSorter.Tests.InMemory.Model;

namespace ImageSorter.Tests.InMemory.ServiceImpl;

public class InMemoryFileStreamService : IFileStreamService
{
    private readonly InMemoryFileSystem _inMemoryFileSystem;

    public InMemoryFileStreamService(InMemoryFileSystem inMemoryFileSystem)
    {
        _inMemoryFileSystem = inMemoryFileSystem;
    }
    
    public Task CopyToAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken)
    {
        _inMemoryFileSystem.CopyFile(sourcePath, destinationPath);
        return Task.CompletedTask;
    }

    public bool FileContentAreEqual(string firstPath, string secondPath)
    {
        return _inMemoryFileSystem.FileContentAreEqual(firstPath, secondPath);
    }
}