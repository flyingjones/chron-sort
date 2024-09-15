using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using ImageSorter.Services.DateParser.MetaData;
using ImageSorter.Services.DateParser.MetaData.ExifTags;
using Moq;
using NUnit.Framework;

namespace ImageSorter.Services.Tests.DateParser.MetaData;

[TestFixture]
public class ExifTagParserTests
{
    [TestCase("jpg", ExifTagId.DateTimeOriginal, "2016:07:18 15:48:54", ExpectedResult = "2016-07-18T15:48:54")]
    [TestCase("jpeg", ExifTagId.DateTimeDigitized, "2016:08:03 19:48:49", ExpectedResult = "2016-08-03T19:48:49")]
    [TestCase("tif", ExifTagId.DateTime, "2016:08:07 21:25:05", ExpectedResult = "2016-08-07T21:25:05")]
    [TestCase("tiff", ExifTagId.DateTimeOriginal, "2017:01:03 14:45:53", ExpectedResult = "2017-01-03T14:45:53")]
    [TestCase("wav", ExifTagId.DateTimeDigitized, "2018:01:03 18:49:02", ExpectedResult = "2018-01-03T18:49:02")]
    [TestCase("png", ExifTagId.DateTime, "2019:01:01 15:31:18", ExpectedResult = "2019-01-01T15:31:18")]
    [TestCase("webp", ExifTagId.DateTimeOriginal, "2021:01:09 15:37:31", ExpectedResult = "2021-01-09T15:37:31")]
    public string TryParseDate_ValidFileEndingValidTagValue(string fileEnding, ExifTagId exifTagId, string tagValue)
    {
        // arrange
        var metaDataTagWrapperMock = new Mock<IMetaDataTagWrapper>();
        metaDataTagWrapperMock.Setup(x => x.HasMetaData).Returns(true);
        metaDataTagWrapperMock.Setup(x => x.GetExifTagValue(exifTagId)).Returns(tagValue);

        var fileHandleMock = new Mock<ILazyFileMetaDataHandle>();
        fileHandleMock.Setup(x => x.GetOrLoadTagWrapper()).Returns(metaDataTagWrapperMock.Object);
        fileHandleMock.Setup(x => x.FileEnding).Returns(fileEnding);

        var parser = new ExifTagParser(exifTagId, 0);
        
        // act & assert
        var canParse = parser.TryParseDate(fileHandleMock.Object, out var result);
        canParse.Should().BeTrue();
        result.Should().NotBeNull();
        return result!.Value.ToString("yyyy-MM-ddTHH:mm:ss");
    }
}