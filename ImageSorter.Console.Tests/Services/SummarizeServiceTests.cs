using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using ImageSorter.DateParsing.Abstractions.Model.MetaData;
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
            new ParsedFileResult
            {
                DateTaken = DateTime.Parse("2024-05-05"),
                FilePath = "/home/images/phone/img01.jpg",
                ParserName = "TestParser"
            },
            new ParsedFileResult
            {
                DateTaken = DateTime.Parse("2024-05-05"),
                FilePath = "/home/images/downloads/img01.jpg",
                ParserName = "TestParser"
            },
            new ParsedFileResult
            {
                DateTaken = DateTime.Parse("2024-05-05"),
                FilePath = "/home/images/backup/img01.jpg",
                ParserName = "TestParser"
            },
            // one file for year 2025, should not be a conflict since it should be sorted in a different directory
            new ParsedFileResult
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
        result.RowShould(0)
            .BeEquivalentTo("Year", "Total", "Conflicts", "Expected Skips", "Expected Count at Destination");

        // year 2024
        result.RowShould(1).BeEquivalentTo("2024", "3", "3", "2", "1");

        // year 2025
        result.RowShould(2).BeEquivalentTo("2025", "1", "0", "0", "1");

        // summary
        result.RowShould(3).BeEquivalentTo("*", "4", "3", "2", "2");
    }

    [TestCase]
    public void DescribeConflicts()
    {
        // arrange
        var writeQueue = new[]
        {
            // three files which have the same month and year and would therefore result in a conflict
            new ParsedFileResult
            {
                DateTaken = DateTime.Parse("2024-05-05"),
                FilePath = "/home/images/phone/img01.jpg",
                ParserName = "TestParser1"
            },
            new ParsedFileResult
            {
                DateTaken = DateTime.Parse("2024-05-06"),
                FilePath = "/home/images/downloads/img01.jpg",
                ParserName = "TestParser2"
            },
            new ParsedFileResult
            {
                DateTaken = DateTime.Parse("2024-05-07"),
                FilePath = "/home/images/backup/img01.jpg",
                ParserName = "TestParser3"
            },
            // one file for year 2025, should not be a conflict since it should be sorted in a different directory
            new ParsedFileResult
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
        var result = service.DescribeConflicts(writeQueue);

        // assert
        var pair = result.Should().ContainSingle().Which;
        pair.Key.Should().Be(Path.GetFullPath("/home/images/sorted/2024/05/img01.jpg"));

        // table header
        pair.Value.RowShould(0).BeEquivalentTo("Source Path", "Parsed Date", "Parser Name");
        // body
        pair.Value.RowShould(1).BeEquivalentTo("/home/images/phone/img01.jpg", "2024-05-05T00:00:00", "TestParser1");
        pair.Value.RowShould(2)
            .BeEquivalentTo("/home/images/downloads/img01.jpg", "2024-05-06T00:00:00", "TestParser2");
        pair.Value.RowShould(3).BeEquivalentTo("/home/images/backup/img01.jpg", "2024-05-07T00:00:00", "TestParser3");
    }

    [Test]
    public void DescribeWrites()
    {
        // arrange
        var fileOperationResults = new[]
        {
            new FileOperationResult
            {
                SourcePath = "/images/img01.jpg",
                DestinationPath = "/images/sorted/2025/01/img01.jpg",
                Status = FileOperationResultStatus.Success,
                FileDate = DateTime.Parse("2025-01-01"),
                ParserName = "ParserA"
            },
            new FileOperationResult
            {
                SourcePath = "/images/img02.jpg",
                DestinationPath = "/images/sorted/2025/01/img02.jpg",
                Status = FileOperationResultStatus.AlreadyInCorrectPlace,
                FileDate = DateTime.Parse("2025-01-01"),
                ParserName = "ParserB"
            },
            new FileOperationResult
            {
                SourcePath = "/images/img03.jpg",
                DestinationPath = "/images/sorted/2025/02/img03.jpg",
                Status = FileOperationResultStatus.Success,
                FileDate = DateTime.Parse("2025-02-01"),
                ParserName = "ParserC"
            }
        };

        var service = _fixture.Create<SummarizeService>();

        // act
        var result = service.DescribeWrites(fileOperationResults).ToArray();

        result.Length.Should().Be(2);
        
        // directory 2025/01
        var pair1 = result[0];
        pair1.Key.Should().Be(Path.GetDirectoryName("/images/sorted/2025/01/"));
        pair1.Value.RowShould(0)
            .BeEquivalentTo("Source Path", "Destination Path", "Status", "Parsed Date", "Parser Name");
        pair1.Value.RowShould(1).BeEquivalentTo("/images/img01.jpg", "/images/sorted/2025/01/img01.jpg", "Success",
            "2025-01-01T00:00:00", "ParserA");
        pair1.Value.RowShould(2).BeEquivalentTo("/images/img02.jpg", "/images/sorted/2025/01/img02.jpg", "AlreadyInCorrectPlace",
            "2025-01-01T00:00:00", "ParserB");
        
        // directory 2025/02
        var pair2 = result[1];
        pair2.Key.Should().Be(Path.GetDirectoryName("/images/sorted/2025/02/"));
        pair2.Value.RowShould(0)
            .BeEquivalentTo("Source Path", "Destination Path", "Status", "Parsed Date", "Parser Name");
        pair2.Value.RowShould(1).BeEquivalentTo("/images/img03.jpg", "/images/sorted/2025/02/img03.jpg", "Success",
            "2025-02-01T00:00:00", "ParserC");
    }

    [Test]
    public void SummarizeWriteResults()
    {
        // arrange
        var fileOperationResults = new[]
        {
            new FileOperationResult
            {
                SourcePath = "/images/img01.jpg",
                DestinationPath = "/images/sorted/2025/01/img01.jpg",
                Status = FileOperationResultStatus.Success,
                FileDate = DateTime.Parse("2025-01-01"),
                ParserName = "ParserA"
            },
            new FileOperationResult
            {
                SourcePath = "/images/img02.jpg",
                DestinationPath = "/images/sorted/2025/01/img02.jpg",
                Status = FileOperationResultStatus.AlreadyInCorrectPlace,
                FileDate = DateTime.Parse("2025-01-01"),
                ParserName = "ParserB"
            },
            new FileOperationResult
            {
                SourcePath = "/images/img03.jpg",
                DestinationPath = "/images/sorted/2025/02/img03.jpg",
                Status = FileOperationResultStatus.Success,
                FileDate = DateTime.Parse("2025-02-01"),
                ParserName = "ParserC"
            }
        };

        var service = _fixture.Create<SummarizeService>();

        // act
        var result = service.SummarizeWriteResults(fileOperationResults);
        
        // assert
        result.RowShould(0).BeEquivalentTo("Year", "Total", "Error", "Skipped", "Already Sorted", "Success",
            "Success (Overwritten)");
        result.RowShould(1).BeEquivalentTo("2025", "3", "0", "0", "1", "2", "0");
        result.RowShould(2).BeEquivalentTo("*", "3", "0", "0", "1", "2", "0");
    }
}