using ImageSorter.FileHandling.FileStream;

namespace ImageSorter.FileWrapper.Tests.FileStream;

[TestFixture]
public class FileStreamServiceTests
{
    private const string TestFileDirectoryPath = "FileStream/TestFiles";
    private string _copiedTestFileName;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _copiedTestFileName = $"{Guid.NewGuid()}.md";
    }

    [TearDown]
    public void TearDown()
    {
        // clean up written files after each test
        var destinationFilePath = Path.Combine(
            TestContext.CurrentContext.TestDirectory, TestFileDirectoryPath, _copiedTestFileName);

        if (File.Exists(destinationFilePath))
        {
            File.Delete(destinationFilePath);
        }
    }

    [TestCase("EmptyFile01.md")]
    [TestCase("FileClass01File01.md")]
    [TestCase("ShortFile01.md")]
    public async Task CopyToAsync(string sourceFileName)
    {
        // arrange
        var service = new FileStreamService();

        var sourceFilePath =
            Path.Combine(TestContext.CurrentContext.TestDirectory, TestFileDirectoryPath, sourceFileName);
        var destinationFilePath =
            Path.Combine(TestContext.CurrentContext.TestDirectory, TestFileDirectoryPath, _copiedTestFileName);

        // act
        await service.CopyToAsync(sourceFilePath, destinationFilePath, default);

        // assert
        var exists = File.Exists(destinationFilePath);
        Assert.That(exists, Is.True);
        
        var areEqual = await service.FileContentAreEqual(sourceFilePath, destinationFilePath, default);
        Assert.That(areEqual, Is.True);
    }

    [TestCase("FileClass01File01.md", "FileClass01File02.md", ExpectedResult = true)]
    [TestCase("EmptyFile01.md", "EmptyFile02.md", ExpectedResult = true)]
    [TestCase("ShortFile01.md", "ShortFile02.md", ExpectedResult = true)]
    [TestCase("ShortFile01.md", "ShortFile03.md", ExpectedResult = false)]
    [TestCase("FileClass01File01.md", "EmptyFile01.md", ExpectedResult = false)]
    [TestCase("ShortFile01.md", "EmptyFile01.md", ExpectedResult = false)]
    [TestCase("FileClass01File01.md", "ShortFile01.md", ExpectedResult = false)]
    public async Task<bool> FileContentAreEqual(string path1, string path2)
    {
        // arrange
        var service = new FileStreamService();

        // act
        var result = await service.FileContentAreEqual(
            Path.Combine(TestContext.CurrentContext.TestDirectory, TestFileDirectoryPath, path1),
            Path.Combine(TestContext.CurrentContext.TestDirectory, TestFileDirectoryPath, path2),
            default);

        // assert
        return result;
    }
}