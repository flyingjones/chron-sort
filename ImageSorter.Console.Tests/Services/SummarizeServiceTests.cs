using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using ImageSorter.Services;

namespace ImageSorter.Tests.Services;

[TestFixture]
public class SummarizeServiceTests
{
    private IFixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
    }
    
    [Test]
    public void SummarizeFileScan()
    {
        // arrange
        var filesToSort = new[] { "img01.jpg", "img02.jpg", "img03.png" };
        var allFiles = filesToSort.Concat(["weirdDocument.md"]).ToArray();

        var service = _fixture.Create<SummarizeService>();
        
        // act
        var result = service.SummarizeFileScan(filesToSort, allFiles);
        
        // assert
        result.ColumnCount.Should().Be(3);
        // header, all, 3 file endings -> 5
        result.RowCount.Should().Be(5);
        
        // header
        result[0, 0].Should().Be("File Ending");
        result[0, 1].Should().Be("Used Count");
        result[0, 2].Should().Be("Ignored Count");
        
        // jpg
        result[1, 0].Should().Be("jpg");
        result[1, 1].Should().Be("2");
        result[1, 2].Should().Be("0");
        
        // png
        result[2, 0].Should().Be("png");
        result[2, 1].Should().Be("1");
        result[2, 2].Should().Be("0");
        
        // md
        result[3, 0].Should().Be("md");
        result[3, 1].Should().Be("0");
        result[3, 2].Should().Be("1");
        
        // summary
        result[4, 0].Should().Be("*");
        result[4, 1].Should().Be("3");
        result[4, 2].Should().Be("1");
    }
}