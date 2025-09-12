using System.Numerics;
using ImageSorter.FileWrapper.Abstractions.FileStream;

namespace ImageSorter.FileHandling.FileStream;

/// <inheritdoc cref="IFileStreamService"/>
public class FileStreamService : IFileStreamService
{
    /// <inheritdoc cref="IFileStreamService.CopyToAsync"/>
    public async Task CopyToAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken)
    {
        await using var sourceFileStream = new System.IO.FileStream(sourcePath, FileMode.Open, FileAccess.Read);
        await using var destFileStream = new System.IO.FileStream(destinationPath, FileMode.Create, FileAccess.Write);

        await sourceFileStream.CopyToAsync(destFileStream, cancellationToken);
    }

    public bool FileContentAreEqual(string firstPath, string secondPath)
    {
        return FastFileContentCompare.FileContentAreEqual(firstPath, secondPath);
    }
}