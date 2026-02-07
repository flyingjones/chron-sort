using ImageSorter.Tests.InMemory.Model;

namespace ImageSorter.Tests.InMemory.Tests;

[TestFixture]
public class InMemoryFileSystemTests
{
    [Test]
    public void CreateDirectory_Works()
    {
        // arrange
        var fs = new InMemoryFileSystem();
        
        // act
        fs.CreateDirectory("usr/home/test");
        fs.CreateDirectory("usr/home/hello");
        fs.CreateDirectory("usr/home/this-is-a-test");

        var dirsInHome = fs.EnumerateDirectories("usr/home").ToArray();
        
        // assert
        Assert.That(dirsInHome, Has.Length.EqualTo(3));
        Assert.That(dirsInHome, Contains.Item("test"));
        Assert.That(dirsInHome, Contains.Item("hello"));
        Assert.That(dirsInHome, Contains.Item("this-is-a-test"));
    }
    
    [Test]
    public void AddSomeFilesTest()
    {
        // arrange
        var fs = new InMemoryFileSystem();
        
        // act
        fs.CreateDirectory("usr/home/test");
        fs.CreateDirectory("usr/home/hello");
        fs.CreateDirectory("usr/home/this-is-a-test");

        var dirsInHome = fs.EnumerateDirectories("usr/home").ToArray();
        
        // assert
        Assert.That(dirsInHome, Has.Length.EqualTo(3));
        Assert.That(dirsInHome, Contains.Item("test"));
        Assert.That(dirsInHome, Contains.Item("hello"));
        Assert.That(dirsInHome, Contains.Item("this-is-a-test"));
    }
}