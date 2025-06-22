using ImageSorter.Markdown.Abstractions.Model;

namespace ImageSorter.Markdown.Helper;

/// <summary>
/// Helper to build a <see cref="MarkdownTable"/>
/// </summary>
public class MarkdownTableBuilder
{
    private readonly List<string?[]> _rows = [];

    /// <summary>
    /// Adds a row to the <see cref="MarkdownTable"/>
    /// </summary>
    /// <remarks>
    /// The first row added is always the header of the resulting <see cref="MarkdownTable"/>
    /// </remarks>
    /// <param name="columnTexts"></param>
    public void AddRow(params string?[] columnTexts)
    {
        _rows.Add(columnTexts);
    }

    /// <summary>
    /// Builds a <see cref="MarkdownTable"/> based on the added rows.
    /// </summary>
    /// <remarks>
    /// The table will have enough columns to fit every added row.
    /// </remarks>
    public MarkdownTable Build()
    {
        // calculate number of columns
        var columnCount = _rows.Max(row => row.Length);

        var table = new MarkdownTable(_rows.Count, columnCount);

        var rowIdx = 0;
        foreach (var row in _rows)
        {
            for (int columnIdx = 0; columnIdx < row.Length; columnIdx++)
            {
                table[rowIdx, columnIdx] = row[columnIdx];
            }

            rowIdx++;
        }

        return table;
    }
}