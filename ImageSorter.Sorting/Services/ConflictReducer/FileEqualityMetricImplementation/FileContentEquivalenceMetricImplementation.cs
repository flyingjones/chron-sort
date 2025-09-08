using ImageSorter.FileWrapper.Abstractions.FileStream;
using ImageSorter.Sorting.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Sorting.Services.ConflictReducer.FileEqualityMetricImplementation;

/// <summary>
/// Implements the <see cref="IFileEquivalenceMetricImplementation"/> by checking the files for byte-wise equality
/// </summary>
/// <remarks>
/// Results in exact matches (only returns true if the file actually contain the same content) but fairly slow,
/// especially for large files.
/// </remarks>
public class FileContentEquivalenceMetricImplementation : IFileEquivalenceMetricImplementation
{
    private readonly IFileStreamService _fileStreamService;
    private readonly ILogger<FileContentEquivalenceMetricImplementation> _logger;

    public FileContentEquivalenceMetricImplementation(
        IFileStreamService fileStreamService,
        ILogger<FileContentEquivalenceMetricImplementation> logger)
    {
        _fileStreamService = fileStreamService;
        _logger = logger;
    }

    public bool FilesAreEquivalent(string path1, string path2)
    {
        _logger.LogDebug("Comparing content of {Path1} and {Path2}", path1, path2);

        if (path1 == path2)
        {
            return true;
        }

        return _fileStreamService.FileContentAreEqual(path1, path2);
    }
}