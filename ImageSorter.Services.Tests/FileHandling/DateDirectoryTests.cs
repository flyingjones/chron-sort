using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using ImageSorter.Services.FileHandling;
using ImageSorter.Services.FileWrapper;
using Moq;
using NUnit.Framework;

namespace ImageSorter.Services.Tests.FileHandling;

[TestFixture]
public class DateDirectoryTests
{
    private IFixture _fixture = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
    }

    [TestCase(true)]
    [TestCase(false)]
    public void CreatePathAndDirs(bool dryRun)
    {
        // arrange
        var options = _fixture.Freeze<DateDirectoryOptions>();
        options.Format = "yyyy/MM";
        options.DestinationPath = "C:/destination/path";
        options.DryRun = dryRun;
        var directoryWrapper = _fixture.Freeze<Mock<IDirectoryWrapper>>();

        var service = _fixture.Create<ConfigurableDateDirectory>();

        // act
        var path1 = service.CreatePathAndDirs(DateTime.Parse("2024-05-01"));
        var path2 = service.CreatePathAndDirs(DateTime.Parse("2024-05-02"));
        var path3 = service.CreatePathAndDirs(DateTime.Parse("2024-05-03"));
        var path4 = service.CreatePathAndDirs(DateTime.Parse("2024-06-01"));
        var path5 = service.CreatePathAndDirs(DateTime.Parse("2024-06-02"));
        var path6 = service.CreatePathAndDirs(DateTime.Parse("2025-01-01"));

        // assert
        path1.Should().Be(Path.GetFullPath($"{options.DestinationPath}/2024/05"));
        path2.Should().Be(Path.GetFullPath($"{options.DestinationPath}/2024/05"));
        path3.Should().Be(Path.GetFullPath($"{options.DestinationPath}/2024/05"));
        path4.Should().Be(Path.GetFullPath($"{options.DestinationPath}/2024/06"));
        path5.Should().Be(Path.GetFullPath($"{options.DestinationPath}/2024/06"));
        path6.Should().Be(Path.GetFullPath($"{options.DestinationPath}/2025/01"));

        if (dryRun)
        {
            directoryWrapper.Verify(x => x.CreateDirectory(It.IsAny<string>()), Times.Never);
        }
        else
        {
            directoryWrapper.Verify(x => x.CreateDirectory(Path.GetFullPath($"{options.DestinationPath}/2024/05")),
                Times.Once);
            directoryWrapper.Verify(x => x.CreateDirectory(Path.GetFullPath($"{options.DestinationPath}/2024/06")),
                Times.Once);
            directoryWrapper.Verify(x => x.CreateDirectory(Path.GetFullPath($"{options.DestinationPath}/2025/01")),
                Times.Once);
            directoryWrapper.Verify(x => x.CreateDirectory(It.IsAny<string>()), Times.Exactly(3));
        }
    }
}