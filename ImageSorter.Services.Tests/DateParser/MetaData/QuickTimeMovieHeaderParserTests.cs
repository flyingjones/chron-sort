using FluentAssertions;
using ImageSorter.Services.DateParser.MetaData;
using ImageSorter.Services.DateParser.MetaData.QuickTimeMovieHeaders;
using Moq;
using NUnit.Framework;

namespace ImageSorter.Services.Tests.DateParser.MetaData;

[TestFixture]
public class QuickTimeMovieHeaderParserTests
{
    [TestCase("mov", (ushort)QuickTimeMovieHeader.CreationTime, "Sat Nov 16 00:00:00 2013", ExpectedResult = "2013-11-16T00:00:00")]
    [TestCase("qt", (ushort)QuickTimeMovieHeader.ModificationTime, "Tue Jan 05 00:00:00 2010", ExpectedResult = "2010-01-05T00:00:00")]
    [TestCase("mp4", (ushort)QuickTimeMovieHeader.CreationTime, "Thu Sep 19 12:29:45 2024", ExpectedResult = "2024-09-19T12:29:45")]
    public string TryParseDate_ValidFileEndingValidTagValue(string fileEnding, QuickTimeMovieHeader header, string tagValue)
    {
        // arrange
        var metaDataTagWrapperMock = new Mock<IMetaDataTagWrapper>();
        metaDataTagWrapperMock.Setup(x => x.HasMetaData).Returns(true);
        metaDataTagWrapperMock.Setup(x => x.GetQuickTimeMovieHeaderValue(header)).Returns(tagValue);

        var fileHandleMock = new Mock<ILazyFileMetaDataHandle>();
        fileHandleMock.Setup(x => x.GetOrLoadTagWrapper()).Returns(metaDataTagWrapperMock.Object);
        fileHandleMock.Setup(x => x.FileEnding).Returns(fileEnding);

        var parser = new QuickTimeMovieHeaderParser(header, 0);
        
        // act & assert
        var canParse = parser.TryParseDate(fileHandleMock.Object, out var result);
        canParse.Should().BeTrue();
        result.Should().NotBeNull();
        return result!.Value.ToString("yyyy-MM-ddTHH:mm:ss");
    }
}