using AutoFixture;
using AutoFixture.AutoMoq;
using ImageSorter.Services.DateTimeWrapper;
using ImageSorter.Services.FileHandling;
using ImageSorter.Services.FileWrapper;
using Moq;
using NUnit.Framework;

namespace ImageSorter.Services.Tests.FileHandling;

[TestFixture]
public class DryRunDestinationWriterTests
{
    private IFixture _fixture = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
    }

    [TestCase(false, false, false)]
    [TestCase(false, false, true)]
    [TestCase(false, true, false)]
    [TestCase(false, true, true)]
    [TestCase(true, false, false)]
    [TestCase(true, false, true)]
    [TestCase(true, true, false)]
    [TestCase(true, true, true)]
    public async Task MoveOrCopy(bool move, bool filesExist, bool overwrite)
    {
        // arrange
        var writeQueue = new[]
        {
            new WriteQueueItem
            {
                DateTaken = DateTime.Parse("2024-06-01"),
                FilePath = "C:/src/path/img-001.png"
            },
            new WriteQueueItem
            {
                DateTaken = DateTime.Parse("2024-06-02"),
                FilePath = "C:/src/path/img-002.png"
            },
            new WriteQueueItem
            {
                DateTaken = DateTime.Parse("2024-07-01"),
                FilePath = "C:/src/path/img-003.png"
            }
        };

        var options = _fixture.Freeze<DestinationWriterOptions>();
        options.DestinationPath = "C:/dest/path";
        options.OverwriteExistingFiles = overwrite;

        var dateTimeProviderMock = _fixture.Freeze<Mock<IDateTimeProvider>>();
        var now = DateTime.Parse("2024-09-11T07:41:00");
        dateTimeProviderMock.Setup(x => x.Now()).Returns(now);
        var directoryWrapperMock = _fixture.Freeze<Mock<IDirectoryWrapper>>();
        var fileWriterMock = _fixture.Freeze<Mock<IFileWrapper>>();
        fileWriterMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(filesExist);
        var streamWriterFactoryMock = _fixture.Freeze<Mock<IBufferedStreamWriterFactory>>();
        var streamWriterMock = _fixture.Freeze<Mock<IStreamWriterWrapper>>();
        streamWriterFactoryMock
            .Setup(x => x.CreateStreamWriter(It.IsAny<string>(), It.IsAny<FileMode>()))
            .Returns(streamWriterMock.Object);

        var service = _fixture.Create<DryRunDestinationWriter>();

        // act
        if (move)
        {
            service.MoveFiles(writeQueue, default);
        }
        else
        {
            await service.CopyFiles(writeQueue, default);
        }

        // assert
        var operation = move ? "Move" : "Copy";
        var optString = (filesExist, overwrite) switch
        {
            (false, _) => string.Empty,
            (true, false) => "[SkipExisting] ",
            (true, true) => "[OverwriteExisting] "
        };
        
        var expectedResultFileName = Path.GetFullPath($"{options.DestinationPath}/sort_{now:yyyy-MM-dd_hh_mm_ss}.txt");
        directoryWrapperMock.Verify(x => x.CreateDirectory(options.DestinationPath), Times.Once);
        streamWriterFactoryMock.Verify(x => x.CreateStreamWriter(expectedResultFileName, FileMode.CreateNew), Times.Once);

        streamWriterMock.Verify(x => x.WriteLine(Path.GetFullPath($"{options.DestinationPath}/2024/06")), Times.Once);
        streamWriterMock.Verify(x => x.WriteLine($"    [{operation}] {optString}C:/src/path/img-001.png [2024-06-01T00:00:00.0000000]"), Times.Once);
        streamWriterMock.Verify(x => x.WriteLine($"    [{operation}] {optString}C:/src/path/img-002.png [2024-06-02T00:00:00.0000000]"), Times.Once);
        
        streamWriterMock.Verify(x => x.WriteLine(Path.GetFullPath($"{options.DestinationPath}/2024/07")), Times.Once);
        streamWriterMock.Verify(x => x.WriteLine($"    [{operation}] {optString}C:/src/path/img-003.png [2024-07-01T00:00:00.0000000]"), Times.Once);
    }
}