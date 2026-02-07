namespace ImageSorter.FileWrapper.Abstractions.FileStream;

public interface IFileStreamService
{
    /// <summary>
    /// Copies a file using the <see cref="FileStream"/>.<see cref="FileStream.CopyToAsync(Stream, CancellationToken)"/> method
    /// </summary>
    Task CopyToAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken);

    /// <summary>
    /// Compares the content of two files using SIMD instructions (byte-wise)
    /// </summary>
    /// <returns>
    /// <c>true</c> only if the files have the same binary content.
    /// </returns>
    Task<bool> FileContentAreEqual(string firstPath, string secondPath, CancellationToken cancellationToken);
}