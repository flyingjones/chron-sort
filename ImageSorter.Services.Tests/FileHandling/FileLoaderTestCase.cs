using ImageSorter.Services.FileHandling;

namespace ImageSorter.Services.Tests.FileHandling;

public class FileLoaderTestCase
{
    public required FileLoaderOptions FileLoaderOptions { get; init; }

    public required string[] FilesOnDisk { get; init; }

    public required string[] ExpectedResult { get; init; }

    public required string TestCaseName { get; init; }

    public override string ToString()
    {
        return TestCaseName;
    }


    public static IEnumerable<FileLoaderTestCase> BuildTestCases()
    {
        yield return new FileLoaderTestCase
        {
            FileLoaderOptions = new FileLoaderOptions
            {
                FileEndings = null,
                SourcePath = "/home/images"
            },
            FilesOnDisk = new[]
            {
                "img1.jpg"
            },
            ExpectedResult = new[]
            {
                "img1.jpg"
            },
            TestCaseName = "Filter null returns file"
        };
        yield return new FileLoaderTestCase
        {
            FileLoaderOptions = new FileLoaderOptions
            {
                FileEndings = Array.Empty<string>(),
                SourcePath = "/home/images"
            },
            FilesOnDisk = new[]
            {
                "img1.jpg"
            },
            ExpectedResult = new[]
            {
                "img1.jpg"
            },
            TestCaseName = "Filter empty returns file"
        };
        yield return new FileLoaderTestCase
        {
            FileLoaderOptions = new FileLoaderOptions
            {
                FileEndings = new[] { "jpg" },
                SourcePath = "/home/images"
            },
            FilesOnDisk = new[]
            {
                "img1.jpg",
                "notes.txt"
            },
            ExpectedResult = new[]
            {
                "img1.jpg"
            },
            TestCaseName = "Filter works"
        };
    }
}