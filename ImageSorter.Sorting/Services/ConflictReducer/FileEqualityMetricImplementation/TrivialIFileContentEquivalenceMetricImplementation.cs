using ImageSorter.Sorting.Abstractions.Services;

namespace ImageSorter.Sorting.Services.ConflictReducer.FileEqualityMetricImplementation;

/// <summary>
/// Trivial implementation: if the paths are equal, the files will also be equal.
/// Otherwise, the files are assumed to be different.
/// </summary>
public class TrivialIFileContentEquivalenceMetricImplementation : IFileEquivalenceMetricImplementation
{
    public bool FilesAreEquivalent(string path1, string path2)
    {
        return path1 == path2;
    }
}