using System.Numerics;

namespace ImageSorter.FileHandling.FileStream;

public static class FastFileContentCompare
{
    /// <summary>
    /// Compares the content of two files using SIMD instructions (byte-wise)
    /// </summary>
    /// <remarks>
    /// Will return <c>true</c> without opening the files if the both point to the same file in the file system.
    /// This is checked by comparing the FileInfo.<see cref="FileInfo.FullName"/> of both files.
    /// <br/>
    /// Will return <c>false</c> without opening the files if the files have different lengths.
    /// </remarks>
    /// <returns>
    /// <c>true</c> only if the files have the same binary content.
    /// </returns>
    public static async Task<bool> FileContentAreEqual(string firstPath, string secondPath, CancellationToken cancellationToken)
    {
        // TODO think (and profile) using the Vector256<> or Vector512<> classes instead
        // profile this on aot and not aot ... maybe there is an aot penalty since the compiler doesn't know the target
        // hardware
        // this is the number of bytes we can compare with one instruction using SIMD hardware instructions
        // should be at least 128 bit = 16 bytes
        int bytesToRead = Vector<byte>.Count;
        
        var first = new FileInfo(firstPath);
        var second = new FileInfo(secondPath);
        
        // if the file size is not equal, the content can't be equal so return directly
        if (first.Length != second.Length)
            return false;
        
        // if the file length is zero, we can instantly return since we know the files are equal (both empty)
        // we know that both are of equal of length since in the other case we would already have returned
        if (first.Length == 0)
            return true;

        // if the full paths are equal, the file content will also be equal, so return directly
        // we don't know if the underlying file system is case-sensitive, so we use the case-sensitive comparison to be
        // safe. The OS should return the same paths on case-insensitive file systems anyway.
        if (string.Equals(first.FullName, second.FullName, StringComparison.Ordinal))
            return true;
        
        // to minimize cpu ops, we read the data in multiple byte sized chunks and compare them using vector ops
        var iterations = (int)Math.Ceiling((double)first.Length / bytesToRead);

        await using var fs1 = first.OpenRead();
        await using var fs2 = second.OpenRead();
        
        var one = new byte[bytesToRead];
        var oneM = new Memory<byte>(one);
        var two = new byte[bytesToRead];
        var twoM = new Memory<byte>(two);

        for (var i = 0; i < iterations; i++)
        {
            // compare the current word of the two files
            // we don't need to check how many bytes have been read since when we reach the end both files read the same
            // amount of new bytes and the arrays is filled with equal values in the remaining space from the last word
            // (or we would have returned false already)
#pragma warning disable CA2022
            // ReSharper disable once MustUseReturnValue
            await fs1.ReadAsync(oneM, cancellationToken);
            // ReSharper disable once MustUseReturnValue
            await fs2.ReadAsync(twoM, cancellationToken);
#pragma warning restore CA2022

            var vecOne = new Vector<byte>(one);
            var vecTwo = new Vector<byte>(two);

            if (vecOne != vecTwo)
                return false;
        }

        return true;
    }
}