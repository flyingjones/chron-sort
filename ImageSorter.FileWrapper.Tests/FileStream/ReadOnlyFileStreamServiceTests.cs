using ImageSorter.FileHandling.FileStream;

namespace ImageSorter.FileWrapper.Tests.FileStream;

[TestFixture]
public class ReadOnlyFileStreamServiceTests
{
    [TestCase("FileClass01File01.md", "FileClass01File02.md", ExpectedResult = true)]
    [TestCase("EmptyFile01.md", "EmptyFile02.md", ExpectedResult = true)]
    [TestCase("ShortFile01.md", "ShortFile02.md", ExpectedResult = true)]
    [TestCase("ShortFile01.md", "ShortFile03.md", ExpectedResult = false)]
    [TestCase("FileClass01File01.md", "EmptyFile01.md", ExpectedResult = false)]
    [TestCase("ShortFile01.md", "EmptyFile01.md", ExpectedResult = false)]
    [TestCase("FileClass01File01.md", "ShortFile01.md", ExpectedResult = false)]
    public bool FileContentAreEqual(string path1, string path2)
    {
        // arrange
        var service = new ReadOnlyFileStreamService();

        // act
        var result = service.FileContentAreEqual(
            Path.Combine(TestContext.CurrentContext.TestDirectory, "FileStream/TestFiles", path1),
            Path.Combine(TestContext.CurrentContext.TestDirectory, "FileStream/TestFiles", path2));

        // assert
        return result;
    }
}