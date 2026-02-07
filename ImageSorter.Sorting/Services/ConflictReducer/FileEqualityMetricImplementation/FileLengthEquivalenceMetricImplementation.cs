using ImageSorter.FileWrapper.Abstractions.File;
using ImageSorter.Sorting.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Sorting.Services.ConflictReducer.FileEqualityMetricImplementation;

/// <summary>
/// Implementation of the <see cref="IFileEquivalenceMetricImplementation"/> which assumes that different paths are
/// of equivalent files if the file length is the same.
/// </summary>
/// <remarks>
/// This metric is relatively fast (only checking file size) but can lead to false positives. Use with this caveat in
/// mind! (For compressed image, audio or video files it should work reasonably well since compression makes the file
/// content entropy somewhat visible in the compressed file length and thus make the exact file size for a given
/// resolution somewhat pseudo-random.)
/// </remarks>
public class FileLengthEquivalenceMetricImplementation : IFileEquivalenceMetricImplementation
{
    private readonly IFileWrapper _fileWrapper;
    private readonly ILogger<FileLengthEquivalenceMetricImplementation> _logger;

    public FileLengthEquivalenceMetricImplementation(
        IFileWrapper fileWrapper,
        ILogger<FileLengthEquivalenceMetricImplementation> logger)
    {
        _fileWrapper = fileWrapper;
        _logger = logger;
    }

    public Task<bool> FilesAreEquivalent(string path1, string path2, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Comparing length of {Path1} and {Path2}", path1, path2);
        
        if (path1 == path2)
        {
            return Task.FromResult(true);
        }

        return Task.FromResult(_fileWrapper.FileSize(path1) == _fileWrapper.FileSize(path2));
    }
}