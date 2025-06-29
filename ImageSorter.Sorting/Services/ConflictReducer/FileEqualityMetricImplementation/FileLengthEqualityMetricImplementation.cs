using ImageSorter.FileWrapper.Abstractions.File;
using ImageSorter.Sorting.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Sorting.Services.ConflictReducer.FileEqualityMetricImplementation;

public class FileLengthEqualityMetricImplementation : IFileEqualityMetricImplementation
{
    private readonly IFileWrapper _fileWrapper;
    private readonly ILogger<FileLengthEqualityMetricImplementation> _logger;

    public FileLengthEqualityMetricImplementation(
        IFileWrapper fileWrapper,
        ILogger<FileLengthEqualityMetricImplementation> logger)
    {
        _fileWrapper = fileWrapper;
        _logger = logger;
    }

    public bool FilesAreEqual(string path1, string path2)
    {
        _logger.LogDebug("Comparing length of {path1} and {path2}", path1, path2);
        
        if (path1 == path2)
        {
            return true;
        }
        
        return _fileWrapper.FileSize(path1) == _fileWrapper.FileSize(path2);
    }
}