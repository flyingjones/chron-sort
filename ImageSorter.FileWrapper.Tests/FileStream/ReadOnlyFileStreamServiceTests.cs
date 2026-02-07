using ImageSorter.FileHandling.FileStream;

namespace ImageSorter.FileWrapper.Tests.FileStream;

[TestFixture]
public class ReadOnlyFileStreamServiceTests
{
    private const string TestFileDirectoryPath = "FileStream/TestFiles";
    private string _copiedTestFileName;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _copiedTestFileName = $"{Guid.NewGuid()}.md";
    }
    
    [TestCase("EmptyFile01.md")]
    [TestCase("FileClass01File01.md")]
    [TestCase("ShortFile01.md")]
    public async Task CopyToAsync(string sourceFileName)
    {
        // arrange
        var service = new ReadOnlyFileStreamService();

        var sourceFilePath =
            Path.Combine(TestContext.CurrentContext.TestDirectory, TestFileDirectoryPath, sourceFileName);
        var destinationFilePath =
            Path.Combine(TestContext.CurrentContext.TestDirectory, TestFileDirectoryPath, _copiedTestFileName);

        // act
        await service.CopyToAsync(sourceFilePath, destinationFilePath, default);

        // assert
        var exists = File.Exists(destinationFilePath);
        Assert.That(exists, Is.False);
    }
    
    [TestCase("FileClass01File01.md", "FileClass01File02.md", ExpectedResult = true)]
    [TestCase("EmptyFile01.md", "EmptyFile02.md", ExpectedResult = true)]
    [TestCase("ShortFile01.md", "ShortFile02.md", ExpectedResult = true)]
    [TestCase("ShortFile01.md", "ShortFile03.md", ExpectedResult = false)]
    [TestCase("FileClass01File01.md", "EmptyFile01.md", ExpectedResult = false)]
    [TestCase("ShortFile01.md", "EmptyFile01.md", ExpectedResult = false)]
    [TestCase("FileClass01File01.md", "ShortFile01.md", ExpectedResult = false)]
    public Task<bool> FileContentAreEqual(string path1, string path2)
    {
        // arrange
        var service = new ReadOnlyFileStreamService();

        // act
        var result = service.FileContentAreEqual(
            Path.Combine(TestContext.CurrentContext.TestDirectory, "FileStream/TestFiles", path1),
            Path.Combine(TestContext.CurrentContext.TestDirectory, "FileStream/TestFiles", path2),
            default);

        // assert
        return result;
    }
}