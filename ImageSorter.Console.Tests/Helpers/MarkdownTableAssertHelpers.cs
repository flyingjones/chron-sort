using FluentAssertions;
using FluentAssertions.Collections;
using ImageSorter.Markdown.Abstractions.Model;

namespace ImageSorter.Tests.Helpers;

public static class MarkdownTableAssertHelpers
{
    public static StringCollectionAssertions RowShould(this MarkdownTable table, int row)
    {
        var rowContent = new string?[table.ColumnCount];

        for (var column = 0; column < table.ColumnCount; column++)
        {
            rowContent[column] = table[row, column];
        }

        return rowContent.Should();
    }
}