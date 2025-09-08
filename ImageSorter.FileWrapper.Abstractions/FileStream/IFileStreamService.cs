namespace ImageSorter.FileWrapper.Abstractions.FileStream;

public interface IFileStreamService
{
    /// <summary>
    /// Copies a file using the <see cref="FileStream"/>.<see cref="FileStream.CopyToAsync(Stream, CancellationToken)"/> method
    /// </summary>
    Task CopyToAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken);

    /// <summary>
    /// Checks that the file content of two files are byte-wise equal
    /// </summary>
    bool FileContentAreEqual(string firstPath, string secondPath);
}