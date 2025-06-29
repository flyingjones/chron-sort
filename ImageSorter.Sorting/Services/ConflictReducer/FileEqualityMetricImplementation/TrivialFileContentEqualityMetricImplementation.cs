using ImageSorter.Sorting.Abstractions.Services;

namespace ImageSorter.Sorting.Services.ConflictReducer.FileEqualityMetricImplementation;

public class TrivialFileContentEqualityMetricImplementation : IFileEqualityMetricImplementation
{
    public bool FilesAreEqual(string path1, string path2)
    {
        return path1 == path2;
    }
}