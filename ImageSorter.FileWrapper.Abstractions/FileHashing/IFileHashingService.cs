namespace ImageSorter.FileWrapper.Abstractions.FileHashing;

public interface IFileHashingService
{
    byte[] ComputeMd5Hash(string filePath);

    string ComputeMd5HashAsString(string filePath, int length);
}