using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using ImageSorter.Services.FileHandling;
using ImageSorter.Services.FileWrapper;
using Moq;
using NUnit.Framework;

namespace ImageSorter.Services.Tests.FileHandling;

[TestFixture]
public class FileLoaderTests
{
    private IFixture _fixture = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
    }

    [TestCaseSource(typeof(FileLoaderTestCase), nameof(FileLoaderTestCase.BuildTestCases))]
    public void GetFilePaths(FileLoaderTestCase testCase)
    {
        // arrange
        var directoryWrapperMock = _fixture.Freeze<Mock<IDirectoryWrapper>>();
        directoryWrapperMock
            .Setup(x => x.GetFiles(testCase.FileLoaderOptions.SourcePath, "*", SearchOption.AllDirectories))
            .Returns(testCase.FilesOnDisk);
        _fixture.Inject<FileLoaderOptions>(testCase.FileLoaderOptions);

        var service = _fixture.Create<FileLoader>();

        // act
        var result = service.GetFilePaths();

        // assert
        result.Should().BeEquivalentTo(testCase.ExpectedResult);

        directoryWrapperMock.Verify(
            x => x.GetFiles(testCase.FileLoaderOptions.SourcePath, "*", SearchOption.AllDirectories), Times.Once);
    }
}