using ImageSorter.FileScanning.Abstractions.Model;
using ImageSorter.FileScanning.Abstractions.Services;
using ImageSorter.FileWrapper.Abstractions.Directory;
using ImageSorter.FileWrapper.Abstractions.Path;

namespace ImageSorter.FileScanning.Services;

public class FileScanningService : IFileScanningService
{
    private readonly IDirectoryWrapper _directoryWrapper;
    private readonly IPathWrapper _pathWrapper;

    public FileScanningService(IDirectoryWrapper directoryWrapper, IPathWrapper pathWrapper)
    {
        _directoryWrapper = directoryWrapper;
        _pathWrapper = pathWrapper;
    }

    public FileScanResultDto ScanPathForFiles(string directoryPath, string[]? fileExtensions)
    {
        var allFiles = _directoryWrapper.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

        if (fileExtensions == null)
        {
            return new FileScanResultDto
            {
                AllFilePaths = allFiles,
                MatchingFilePaths = allFiles
            };
        }
        
        // we assume, that the user wants a case-insensitive file ending comparison here
        // even if the file system is case-sensitive
        // since probably .JPG is the same file format than .jpg ...
        var lowerCaseFileEndings = fileExtensions.Select(x => x.ToLower()).ToArray();
        var matchingFilePaths = allFiles
            .Select(x => new { Path = x, Extension = _pathWrapper.GetExtension(x).ToLower() })
            .Where(x => lowerCaseFileEndings.Contains(x.Extension))
            .Select(x => x.Path)
            .ToArray();

        return new FileScanResultDto
        {
            AllFilePaths = allFiles,
            MatchingFilePaths = matchingFilePaths
        };
    }
}