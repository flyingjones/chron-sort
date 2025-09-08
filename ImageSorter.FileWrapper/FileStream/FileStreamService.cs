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
        const int bytesToRead = sizeof(long);
        
        var first = new FileInfo(firstPath);
        var second = new FileInfo(secondPath);
        
        // if the file size is not equal, the content can't be equal so return directly
        if (first.Length != second.Length)
            return false;

        // if the full paths are equal, the file content will also be equal, so return directly
        // we don't know if the underlying file system is case-sensitive, so we use the case-sensitive comparison to be
        // safe. The OS should return the same paths on case-insensitive file systems anyway.
        if (string.Equals(first.FullName, second.FullName, StringComparison.Ordinal))
            return true;
        
        // to minimize cpu ops, we read the data in 64 bit chunks and compare them as longs
        var iterations = (int)Math.Ceiling((double)first.Length / bytesToRead);

        using var fs1 = first.OpenRead();
        using var fs2 = second.OpenRead();
        
        var one = new byte[bytesToRead];
        var two = new byte[bytesToRead];

        for (var i = 0; i < iterations; i++)
        {
            // compare the current 64 bit word of the two files
            // we don't need to check how many bytes have been read since when we reach the end both files read the same
            // amount of new bytes and the arrays is filled with equal values in the remaining space from the last word
            // (or we would have returned false already)
#pragma warning disable CA2022
            // ReSharper disable once MustUseReturnValue
            fs1.Read(one, 0, bytesToRead);
            // ReSharper disable once MustUseReturnValue
            fs2.Read(two, 0, bytesToRead);
#pragma warning restore CA2022

            if (BitConverter.ToInt64(one,0) != BitConverter.ToInt64(two,0))
                return false;
        }

        return true;
    }
}