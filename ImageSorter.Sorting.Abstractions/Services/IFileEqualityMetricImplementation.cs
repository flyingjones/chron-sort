namespace ImageSorter.Sorting.Abstractions.Services;

public interface IFileEqualityMetricImplementation
{
    bool FilesAreEqual(string path1, string path2);
}