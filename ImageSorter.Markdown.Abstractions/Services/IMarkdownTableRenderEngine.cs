using ImageSorter.Markdown.Abstractions.Model;

namespace ImageSorter.Markdown.Abstractions.Services;

public interface IMarkdownTableRenderEngine
{
    string Render(MarkdownTable table, bool escape = true);
}