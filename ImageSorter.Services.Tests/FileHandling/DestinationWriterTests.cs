using AutoFixture;
using AutoFixture.AutoMoq;
using ImageSorter.Services.FileHandling;
using ImageSorter.Services.FileWrapper;
using Moq;
using NUnit.Framework;

namespace ImageSorter.Services.Tests.FileHandling;

[TestFixture]
public class DestinationWriterTests
{
    private IFixture _fixture = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
    }

    [Test]
    public void MoveFile_NothingExists()
    {
        // arrange
        const string sourcePath = "source/path/img1.jpg";
        const string destPath = "dest/path";
        var date = DateTime.Parse("2024-05-08");

        var directoryMock = _fixture.Freeze<Mock<IDirectoryWrapper>>();
        var fileMock = _fixture.Freeze<Mock<IFileWrapper>>();
        fileMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
        var options = _fixture.Freeze<DestinationWriterOptions>();
        options.DestinationPath = destPath;

        var service = _fixture.Create<DestinationWriter>();
        
        // act
        service.MoveFile(sourcePath, date);
        
        // assert
        directoryMock.Verify(x => x.CreateDirectory($"{destPath}/2024"), Times.Once);
        directoryMock.Verify(x => x.CreateDirectory($"{destPath}/2024/05"), Times.Once);
        fileMock.Verify(x => x.Exists($"{destPath}/2024/05/img1.jpg"), Times.Once);
        fileMock.Verify(x => x.Move(sourcePath, $"{destPath}/2024/05/img1.jpg", It.IsAny<bool>()), Times.Once);
    }
    
    [TestCase(true)]
    [TestCase(false)]
    public void MoveFile_FileExists(bool overwrite)
    {
        // arrange
        const string sourcePath = "source/path/img1.jpg";
        const string destPath = "dest/path";
        var date = DateTime.Parse("2024-05-08");

        var directoryMock = _fixture.Freeze<Mock<IDirectoryWrapper>>();
        var fileMock = _fixture.Freeze<Mock<IFileWrapper>>();
        fileMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        var options = _fixture.Freeze<DestinationWriterOptions>();
        options.DestinationPath = destPath;
        options.OverwriteExistingFiles = overwrite;

        var service = _fixture.Create<DestinationWriter>();
        
        // act
        service.MoveFile(sourcePath, date);
        
        // assert
        directoryMock.Verify(x => x.CreateDirectory($"{destPath}/2024"), Times.Once);
        directoryMock.Verify(x => x.CreateDirectory($"{destPath}/2024/05"), Times.Once);
        fileMock.Verify(x => x.Exists($"{destPath}/2024/05/img1.jpg"), Times.Once);

        if (overwrite)
        {
            fileMock.Verify(x => x.Move(sourcePath, $"{destPath}/2024/05/img1.jpg", true), Times.Once);
        }
        else
        {
            fileMock.Verify(x => x.Move(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }
    }
}