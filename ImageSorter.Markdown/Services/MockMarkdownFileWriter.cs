using ImageSorter.Markdown.Abstractions.Model;
using ImageSorter.Markdown.Abstractions.Services;

namespace ImageSorter.Markdown.Services;

public class MockMarkdownFileWriter : IMarkdownFileWriter
{
    public void WriteHeading(MarkdownHeading headingType, string text)
    {

    }

    public void WriteTable(MarkdownTable table)
    {

    }

    public void Write(string text)
    {

    }

    public void WriteLine(string? text = null)
    {

    }

    public void Dispose()
    {
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}