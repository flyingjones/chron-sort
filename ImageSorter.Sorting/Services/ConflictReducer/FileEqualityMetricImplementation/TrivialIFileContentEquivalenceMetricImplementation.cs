using ImageSorter.Sorting.Abstractions.Services;

namespace ImageSorter.Sorting.Services.ConflictReducer.FileEqualityMetricImplementation;

/// <summary>
/// Trivial implementation: if the paths are equal, the files will also be equal.
/// Otherwise, the files are assumed to be different.
/// </summary>
public class TrivialIFileContentEquivalenceMetricImplementation : IFileEquivalenceMetricImplementation
{
    public Task<bool> FilesAreEquivalent(string path1, string path2, CancellationToken cancellationToken)
    {
        return Task.FromResult(path1 == path2);
    }
}