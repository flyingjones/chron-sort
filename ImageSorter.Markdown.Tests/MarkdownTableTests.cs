using FluentAssertions;
using ImageSorter.Markdown.Helper;
using ImageSorter.Markdown.Model;

namespace ImageSorter.Markdown.Tests;

[TestFixture]
public class MarkdownTableTests
{
    [Test]
    public void TestSimpleTable()
    {
        // arrange
        var table = new MarkdownTable(3, 3);
        table.SetTextAlignment(0, HorizontalTextAlignment.Left);
        table.SetTextAlignment(1, HorizontalTextAlignment.Center);
        table.SetTextAlignment(2, HorizontalTextAlignment.Right);

        table[0, 0] = "Header Text 0";
        table[0, 1] = "Header Text 1";
        table[0, 2] = "Header Text 2";

        table[1, 0] = "this is a very long text to test the auto alignment";
        table[1, 1] = "this is a very long text to test the auto alignment";
        table[1, 2] = "this is a very long text to test the auto alignment";
        
        table[2, 0] = "sas";
        table[2, 1] = "sas";
        table[2, 2] = "sas";
        
        // act
        var result = table.Render();
        
        // assert
        result.Should().NotBeNull();
    }

    [Test]
    public void TestBuilder()
    {
        // arrange
        var builder = new MarkdownTableBuilder();
        
        builder.AddRow("Header 0", "Header 1");
        builder.AddRow("Body 0", "Body 1", "Body 3", "Body 4");
        builder.AddRow();
        
        // act
        var table = builder.Build();

        // assert
        table.RowCount.Should().Be(3);
        table.ColumnCount.Should().Be(4);
    }
}