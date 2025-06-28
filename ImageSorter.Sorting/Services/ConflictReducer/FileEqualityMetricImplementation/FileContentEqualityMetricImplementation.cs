using ImageSorter.FileWrapper.Abstractions.FileStream;
using ImageSorter.Sorting.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Sorting.Services.ConflictReducer.FileEqualityMetricImplementation;

public class FileContentEqualityMetricImplementation : IFileEqualityMetricImplementation
{
    private readonly IFileStreamService _fileStreamService;
    private readonly ILogger<FileContentEqualityMetricImplementation> _logger;

    public FileContentEqualityMetricImplementation(
        IFileStreamService fileStreamService,
        ILogger<FileContentEqualityMetricImplementation> logger)
    {
        _fileStreamService = fileStreamService;
        _logger = logger;
    }

    public bool FilesAreEqual(string path1, string path2)
    {
        _logger.LogDebug("Comparing content of {path1} and {path2}", path1, path2);
        return _fileStreamService.FileContentAreEqual(path1, path2);
    }
}