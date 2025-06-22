using ImageSorter.FileHandling.FileStream;
using ImageSorter.FileWrapper.Abstractions.FileStream;
using ImageSorter.Markdown.Abstractions.Model;
using ImageSorter.Markdown.Abstractions.Services;

namespace ImageSorter.Markdown.Services;

/// <inheritdoc cref="IMarkdownFileWriter"/>
public sealed class MarkdownFileWriter : IMarkdownFileWriter
{
    private readonly IStreamWriterWrapper _fileStream;
    private readonly bool _escapeTables;
    private readonly IMarkdownTableRenderEngine _markdownTableRenderEngine;

    public MarkdownFileWriter(
        IStreamWriterWrapper fileStream,
        IMarkdownTableRenderEngine markdownTableRenderEngine,
        bool escapeTables)
    {
        _fileStream = fileStream;
        _escapeTables = escapeTables;
        _markdownTableRenderEngine = markdownTableRenderEngine;
    }

    /// <inheritdoc cref="IMarkdownFileWriter.WriteHeading"/>
    public void WriteHeading(MarkdownHeading headingType, string text)
    {
        _fileStream.Write($"{new string('#', (int)headingType)} {text}{Environment.NewLine}{Environment.NewLine}");
    }

    /// <inheritdoc cref="IMarkdownFileWriter.WriteTable"/>
    public void WriteTable(MarkdownTable table)
    {
        _fileStream.WriteLine(_markdownTableRenderEngine.Render(table, _escapeTables));
    }

    /// <inheritdoc cref="IMarkdownFileWriter.Write"/>
    public void Write(string text)
    {
        _fileStream.Write(text);
    }

    /// <inheritdoc cref="IMarkdownFileWriter.WriteLine"/>
    public void WriteLine(string? text = null)
    {
        _fileStream.WriteLine(text);
    }

    public void Dispose()
    {
        _fileStream.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _fileStream.DisposeAsync();
    }
}