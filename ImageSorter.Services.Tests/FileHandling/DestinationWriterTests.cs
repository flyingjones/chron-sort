using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using ImageSorter.DateParsing.Abstractions.Model.MetaData;
using ImageSorter.FileHandling.Directory;
using ImageSorter.FileHandling.File;
using ImageSorter.FileHandling.FileStream;
using ImageSorter.FileWrapper.Abstractions.Directory;
using ImageSorter.FileWrapper.Abstractions.File;
using ImageSorter.FileWrapper.Abstractions.FileStream;
using ImageSorter.Services.FileHandling;
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

        var fileMock = _fixture.Freeze<Mock<IFileWrapper>>();
        fileMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
        var options = _fixture.Freeze<DestinationWriterOptions>();
        options.DestinationPath = destPath;
        var dateDirectoryOptions = _fixture.Freeze<DateDirectoryOptions>();
        dateDirectoryOptions.DestinationPath = destPath;
        dateDirectoryOptions.Format = "yyyy/MM";

        _fixture.Inject<IDateDirectory>(_fixture.Create<ConfigurableDateDirectory>());
        var service = _fixture.Create<DestinationWriter>();

        // act
        var result = service.MoveFile(sourcePath, date);

        // assert
        fileMock.Verify(x => x.Exists(Path.GetFullPath($"{destPath}/2024/05/img1.jpg")), Times.Once);
        fileMock.Verify(x => x.Move(sourcePath, Path.GetFullPath($"{destPath}/2024/05/img1.jpg"), It.IsAny<bool>()),
            Times.Once);
        result.Status.Should().Be(FileOperationResultStatus.Success);
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
        var dateDirectoryOptions = _fixture.Freeze<DateDirectoryOptions>();
        dateDirectoryOptions.DestinationPath = destPath;
        dateDirectoryOptions.Format = "yyyy/MM";

        _fixture.Inject<IDateDirectory>(_fixture.Create<ConfigurableDateDirectory>());
        var service = _fixture.Create<DestinationWriter>();

        // act
        var result = service.MoveFile(sourcePath, date);

        // assert
        fileMock.Verify(x => x.Exists(Path.GetFullPath($"{destPath}/2024/05/img1.jpg")), Times.Once);

        if (overwrite)
        {
            fileMock.Verify(x => x.Move(sourcePath, Path.GetFullPath($"{destPath}/2024/05/img1.jpg"), true),
                Times.Once);
            result.Status.Should().Be(FileOperationResultStatus.OverwriteSuccess);
        }
        else
        {
            fileMock.Verify(x => x.Move(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
            result.Status.Should().Be(FileOperationResultStatus.Skipped);
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CopyFile_FileExists(bool overwrite)
    {
        // arrange
        const string sourcePath = "C:/source/path/img1.jpg";
        const string destPath = "C:/dest/path";
        var date = DateTime.Parse("2024-05-08");

        var fileMock = _fixture.Freeze<Mock<IFileWrapper>>();
        fileMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(true);
        var fileStreamServiceMock = _fixture.Freeze<Mock<IFileStreamService>>();
        var options = _fixture.Freeze<DestinationWriterOptions>();
        options.DestinationPath = destPath;
        options.OverwriteExistingFiles = overwrite;
        var dateDirectoryOptions = _fixture.Freeze<DateDirectoryOptions>();
        dateDirectoryOptions.DestinationPath = destPath;
        dateDirectoryOptions.Format = "yyyy/MM";

        _fixture.Inject<IDateDirectory>(_fixture.Create<ConfigurableDateDirectory>());
        var service = _fixture.Create<DestinationWriter>();

        // act
        var result = await service.CopyFile(sourcePath, date, default);

        // assert
        fileMock.Verify(x => x.Exists(Path.GetFullPath($"{destPath}/2024/05/img1.jpg")), Times.Once);

        if (overwrite)
        {
            fileStreamServiceMock.Verify(
                x => x.CopyToAsync(sourcePath, Path.GetFullPath($"{destPath}/2024/05/img1.jpg"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            result.Status.Should().Be(FileOperationResultStatus.OverwriteSuccess);
        }
        else
        {
            fileStreamServiceMock.Verify(
                x => x.CopyToAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            result.Status.Should().Be(FileOperationResultStatus.Skipped);
        }
    }

    [Test]
    public void MoveFiles()
    {
        // arrange
        var writeQueue = new[]
        {
            new ParsedFileResult
            {
                FilePath = "/source/path/img1.jpg", DateTaken = DateTime.Parse("2024-05-01"),
            },
            new ParsedFileResult
            {
                FilePath = "/source/path/img2.jpg", DateTaken = DateTime.Parse("2024-05-01"),
            },
            new ParsedFileResult
            {
                FilePath = "/source/path/img3.jpg", DateTaken = DateTime.Parse("2024-06-01"),
            },
            new ParsedFileResult
            {
                FilePath = "/source/path/img4.jpg", DateTaken = DateTime.Parse("2025-06-01"),
            }
        };

        const string destPath = "/dest/path";
        var directoryMock = _fixture.Freeze<Mock<IDirectoryWrapper>>();
        var fileMock = _fixture.Freeze<Mock<IFileWrapper>>();
        fileMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
        var options = _fixture.Freeze<DestinationWriterOptions>();
        options.DestinationPath = destPath;
        options.OverwriteExistingFiles = false;
        options.From = null;
        options.To = null;
        var dateDirectoryOptions = _fixture.Freeze<DateDirectoryOptions>();
        dateDirectoryOptions.DestinationPath = destPath;
        dateDirectoryOptions.Format = "yyyy/MM";

        _fixture.Inject<IDateDirectory>(_fixture.Create<ConfigurableDateDirectory>());
        var service = _fixture.Create<DestinationWriter>();

        // act
        service.MoveFiles(writeQueue, default);

        // assert
        fileMock.Verify(x => x.Move("/source/path/img1.jpg", Path.GetFullPath($"{destPath}/2024/05/img1.jpg"), false));
        fileMock.Verify(x => x.Move("/source/path/img2.jpg", Path.GetFullPath($"{destPath}/2024/05/img2.jpg"), false));
        fileMock.Verify(x => x.Move("/source/path/img3.jpg", Path.GetFullPath($"{destPath}/2024/06/img3.jpg"), false));
        fileMock.Verify(x => x.Move("/source/path/img4.jpg", Path.GetFullPath($"{destPath}/2025/06/img4.jpg"), false));
    }

    [Test]
    public async Task CopyFiles()
    {
        // arrange
        var writeQueue = new[]
        {
            new ParsedFileResult
            {
                FilePath = "/source/path/img1.jpg", DateTaken = DateTime.Parse("2024-05-01"),
            },
            new ParsedFileResult
            {
                FilePath = "/source/path/img2.jpg", DateTaken = DateTime.Parse("2024-05-01"),
            },
            new ParsedFileResult
            {
                FilePath = "/source/path/img3.jpg", DateTaken = DateTime.Parse("2024-06-01"),
            },
            new ParsedFileResult
            {
                FilePath = "/source/path/img4.jpg", DateTaken = DateTime.Parse("2025-06-01"),
            }
        };

        const string destPath = "/dest/path";
        var fileMock = _fixture.Freeze<Mock<IFileWrapper>>();
        fileMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);
        var fileStreamServiceMock = _fixture.Freeze<Mock<IFileStreamService>>();
        var options = _fixture.Freeze<DestinationWriterOptions>();
        options.DestinationPath = destPath;
        options.OverwriteExistingFiles = false;
        options.From = null;
        options.To = null;
        var dateDirectoryOptions = _fixture.Freeze<DateDirectoryOptions>();
        dateDirectoryOptions.DestinationPath = destPath;
        dateDirectoryOptions.Format = "yyyy/MM";

        _fixture.Inject<IDateDirectory>(_fixture.Create<ConfigurableDateDirectory>());
        var service = _fixture.Create<DestinationWriter>();

        // act
        await service.CopyFiles(writeQueue, default);

        // assert

        fileStreamServiceMock.Verify(x =>
            x.CopyToAsync("/source/path/img1.jpg", Path.GetFullPath($"{destPath}/2024/05/img1.jpg"),
                It.IsAny<CancellationToken>()));
        fileStreamServiceMock.Verify(x =>
            x.CopyToAsync("/source/path/img2.jpg", Path.GetFullPath($"{destPath}/2024/05/img2.jpg"),
                It.IsAny<CancellationToken>()));
        fileStreamServiceMock.Verify(x =>
            x.CopyToAsync("/source/path/img3.jpg", Path.GetFullPath($"{destPath}/2024/06/img3.jpg"),
                It.IsAny<CancellationToken>()));
        fileStreamServiceMock.Verify(x =>
            x.CopyToAsync("/source/path/img4.jpg", Path.GetFullPath($"{destPath}/2025/06/img4.jpg"),
                It.IsAny<CancellationToken>()));
    }
}