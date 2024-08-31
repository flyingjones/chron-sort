using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using ImageSorter.Services.DateParser;
using NUnit.Framework;

namespace ImageSorter.Services.Tests.DateParser;

[TestFixture]
public class FileNameDateParserTests
{
    [TestCase(".*(?<year>20[0-9]{2}|19[0-9]{2})-(?<month>0[0-9]|1[0-9])-(?<day>0[0-9]|1[0-9]|2[0-9]|3[0-1]).*", "2020-01-02_001.jpg", ExpectedResult = "2020-01-02")]
    [TestCase(".*(?<year>20[0-9]{2}|19[0-9]{2})(?<month>0[0-9]|1[0-9])(?<day>0[0-9]|1[0-9]|2[0-9]|3[0-1]).*", "20200102_001.jpg", ExpectedResult = "2020-01-02")]
    [TestCase(".*(?<year>20[0-9]{2}|19[0-9]{2})(?<day>0[0-9]|1[0-9])(?<month>0[0-9]|1[0-9]|2[0-9]|3[0-1]).*", "20200102_001.jpg", ExpectedResult = "2020-02-01")]
    public string? TryParseDateFromFileName([StringSyntax(StringSyntaxAttribute.Regex)] string fileNameRegex, string fileName)
    {
        // arrange
        var parser = new FilenameDateParser(fileNameRegex, 0);
        
        // act
        var result = parser.TryParseDateFromFileName(fileName, out var resultingDate);

        if (result)
        {
            Debug.Assert(resultingDate != null, nameof(resultingDate) + " != null");
            return resultingDate.Value.ToString("yyyy-MM-dd");
        }

        return null;
    }

    [TestCase("", ExpectedResult = "Should contain three named capture groups (Parameter 'fileNameRegex')")]
    [TestCase("(?<year>A)(?<month>B)(?<blub>C)", ExpectedResult = "Missing named capture group \"day\" (Parameter 'fileNameRegex')")]
    [TestCase("(?<year>A)(?<blub>B)(?<day>C)", ExpectedResult = "Missing named capture group \"month\" (Parameter 'fileNameRegex')")]
    [TestCase("(?<blub>A)(?<month>B)(?<day>C)", ExpectedResult = "Missing named capture group \"year\" (Parameter 'fileNameRegex')")]
    public string? FileNameDateParser_Constructor_InvalidRegex_Trows(
        [StringSyntax(StringSyntaxAttribute.Regex)] string fileNameRegex)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            var filenameDateParser = new FilenameDateParser(fileNameRegex, 0);
        });

        exception.Should().NotBeNull();
        exception?.ParamName.Should().Be("fileNameRegex");
        
        return exception?.Message;
    }
}