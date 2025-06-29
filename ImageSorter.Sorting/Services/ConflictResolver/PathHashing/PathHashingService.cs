using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using ImageSorter.Sorting.Model;

namespace ImageSorter.Sorting.Services.ConflictResolver.PathHashing;

public class PathHashingService : IPathHashingService
{
    private readonly char[] _charsetArray;
    private readonly FileSystemConfig _config;

    public PathHashingService(FileSystemConfig config)
    {
        _config = config;
        // this charset is exactly 32 long -> we can neatly divide the hash into it:
        // one byte are 256 possible values, and we have 32 -> one byte of the hash becomes 2 chars
        _charsetArray = "abcdefghijklmnopqrstuvwxyz012345".ToCharArray();
    }


    public string HashPath(string path, int length)
    {
        var valueToHash = _config.IsCaseSensitive ? path : path.ToLower();

        var byteArrayToHash = Encoding.UTF8.GetBytes(valueToHash);
        var hash = CryptographicOperations.HashData(HashAlgorithmName.SHA256, byteArrayToHash);

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
}