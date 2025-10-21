using System.Security.Cryptography;
using ImageSorter.FileWrapper.Abstractions.FileHashing;

namespace ImageSorter.FileHandling.FileHashing;

public class FileHashingService : IFileHashingService, IDisposable
{
    private readonly MD5 _md5;
    private readonly char[] _charsetArray;

    public FileHashingService()
    {
        _md5 = MD5.Create();
        // this charset is exactly 32 long -> we can neatly divide the hash into it:
        // one byte are 256 possible values, and we have 32 -> one byte of the hash becomes 2 chars
        _charsetArray = "abcdefghijklmnopqrstuvwxyz012345".ToCharArray();
    }

    public byte[] ComputeMd5Hash(string filePath)
    {
        using var stream = System.IO.File.OpenRead(filePath);
        return _md5.ComputeHash(stream);
    }

    public string ComputeMd5HashAsString(string filePath, int length)
    {
        var hash = ComputeMd5Hash(filePath);
        var resultArray = new char[length < hash.Length * 2 ? length : hash.Length * 2];

        for (var i = 0; i < resultArray.Length; i++)
        {
            // one byte = 8 bits, read each byte twice
            var hashArrayIndex = i / 2;
            var hashShiftIndex = i % 2;

            var currentHashByte = hash[hashArrayIndex];
            // we read 4 bits at a time
            var currentHashValue = (currentHashByte & (0b1111 << (hashShiftIndex * 4))) >> (hashShiftIndex * 4);
            resultArray[i] = _charsetArray[currentHashValue];
        }

        return new string(resultArray);
    }

    public void Dispose()
    {
        _md5.Dispose();
    }
}