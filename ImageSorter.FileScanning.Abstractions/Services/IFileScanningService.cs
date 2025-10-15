using ImageSorter.FileScanning.Abstractions.Model;

namespace ImageSorter.FileScanning.Abstractions.Services;

public interface IFileScanningService
{
    /// <summary>
    /// Find all files in a directory (and its subdirectories recursively).
    /// </summary>
    /// <remarks>
    /// Will filter the result by the passed <paramref name="fileExtensions"/> and return the filtered results in
    /// the <see cref="FileScanResultDto.MatchingFilePaths"/>. Will also return all found files.
    /// <br/>
    /// The file extension matching is case-insensitive!
    /// </remarks>
    /// <param name="directoryPath">Path to search</param>
    /// <param name="fileExtensions">Collection of file extensions. Pass null to return all files</param>
    /// <returns></returns>
    FileScanResultDto ScanPathForFiles(string directoryPath, string[]? fileExtensions);
}