using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using ImageSorter.FileHandling.Directory;
using ImageSorter.Services;
using ImageSorter.Services.FileHandling;
using ImageSorter.Tests.Helpers;

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
        result.RowShould(0).BeEquivalentTo("File Ending", "Used Count", "Ignored Count");

        // jpg
        result.RowShould(1).BeEquivalentTo("jpg", "2", "0");

        // png
        result.RowShould(2).BeEquivalentTo("png", "1", "0");

        // md
        result.RowShould(3).BeEquivalentTo("md", "0", "1");

        // summary
        result.RowShould(4).BeEquivalentTo("*", "3", "1");
    }

    [Test]
    public void SummarizeConflicts()
    {
        // arrange
        var writeQueue = new[]
        {
            // three files which have the same month and year and would therefore result in a conflict
            new WriteQueueItem
            {
                DateTaken = DateTime.Parse("2024-05-05"),
                FilePath = "/home/images/phone/img01.jpg",
                ParserName = "TestParser"
            },
            new WriteQueueItem
            {
                DateTaken = DateTime.Parse("2024-05-05"),
                FilePath = "/home/images/downloads/img01.jpg",
                ParserName = "TestParser"
            },
            new WriteQueueItem
            {
                DateTaken = DateTime.Parse("2024-05-05"),
                FilePath = "/home/images/backup/img01.jpg",
                ParserName = "TestParser"
            },
            // one file for year 2025, should not be a conflict since it should be sorted in a different directory
            new WriteQueueItem
            {
                DateTaken = DateTime.Parse("2025-01-11"),
                FilePath = "/home/images/memes/img01.jpg"
            }
        };
        
        _fixture.Inject<IDateDirectory>(new ConfigurableDateDirectory(new DateDirectoryOptions
        {
            DestinationPath = "/home/images/sorted",
            Format = "yyyy/MM"
        }, new ReadOnlyDirectoryWrapper()));
        var service = _fixture.Create<SummarizeService>();
        
        // act
        var result = service.SummarizeConflicts(writeQueue);
        
        // assert
        result.RowCount.Should().Be(4);
        result.ColumnCount.Should().Be(5);
        
        // header
        result.RowShould(0).BeEquivalentTo("Year", "Total", "Conflicts", "Expected Skips", "Expected Count at Destination");
        
        // year 2024
        result.RowShould(1).BeEquivalentTo("2024", "3", "3", "2", "1");
        
        // year 2025
        result.RowShould(2).BeEquivalentTo("2025", "1", "0", "0", "1");
        
        // summary
        result.RowShould(3).BeEquivalentTo("*", "4", "3", "2", "2");
    }
}