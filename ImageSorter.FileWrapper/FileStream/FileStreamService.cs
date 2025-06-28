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
        
        if (first.Length != second.Length)
            return false;

        if (string.Equals(first.FullName, second.FullName, StringComparison.OrdinalIgnoreCase))
            return true;
        
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