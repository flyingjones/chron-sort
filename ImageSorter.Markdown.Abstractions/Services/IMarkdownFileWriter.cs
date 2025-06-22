using ImageSorter.Markdown.Abstractions.Model;

namespace ImageSorter.Markdown.Abstractions.Services;

/// <summary>
/// Interface for writing data to a markdown formatted file
/// </summary>
public interface IMarkdownFileWriter: IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Write a heading and two new lines to the filestream
    /// </summary>
    void WriteHeading(MarkdownHeading headingType, string text);

    /// <summary>
    /// Write the table and a new line to the filestream
    /// </summary>
    void WriteTable(MarkdownTable table);

    /// <summary>
    /// Write the <paramref name="text"/> to the file
    /// </summary>
    void Write(string text);

    /// <summary>
    /// Write the <paramref name="text"/> and a new line to the file
    /// </summary>
    void WriteLine(string? text = null);

}