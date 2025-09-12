using ImageSorter.FileWrapper.Abstractions.FileStream;

namespace ImageSorter.FileHandling.FileStream;

public class ReadOnlyFileStreamService : IFileStreamService
{
    /// <summary>
    /// Doesn't do anything
    /// </summary>
    public Task CopyToAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    
    public bool FileContentAreEqual(string firstPath, string secondPath)
    {
        return FastFileContentCompare.FileContentAreEqual(firstPath, secondPath);
    }
}