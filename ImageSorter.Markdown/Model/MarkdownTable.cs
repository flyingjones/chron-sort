using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ImageSorter.Markdown.Model;

/// <summary>
/// Represents a table which can be rendered in Markdown
/// </summary>
public class MarkdownTable
{
    private readonly string?[,] _content;
    private readonly HorizontalTextAlignment[] _columnAlignments;
    
    /// <summary>
    /// Allocate a new table
    /// </summary>
    /// <param name="rows">Number of rows</param>
    /// <param name="columns">Number of columns</param>
    public MarkdownTable(int rows, int columns)
    {
        _content = new string[rows, columns];
        _columnAlignments = new HorizontalTextAlignment[columns];
        Array.Fill(_columnAlignments, HorizontalTextAlignment.Default);
    }

    /// <summary>
    /// Access the content of a cell by row and column index
    /// </summary>
    /// <remarks>Must be in bounds!</remarks>
    /// <param name="row">Row index</param>
    /// <param name="column">Column index</param>
    public string? this[int row, int column]
    {
        get => _content[row, column];
        set => _content[row, column] = value;
    }

    public int RowCount => _content.GetLength(0);
    
    public int ColumnCount => _content.GetLength(1);

    /// <summary>
    /// Gets the current <see cref="HorizontalTextAlignment"/> of a column
    /// </summary>
    /// <param name="column">Column index</param>
    public HorizontalTextAlignment GetTextAlignment(int column)
    {
        return _columnAlignments[column];
    }
    
    /// <summary>
    /// Sets the <see cref="HorizontalTextAlignment"/> of all columns
    /// </summary>
    public void SetTextAlignment(HorizontalTextAlignment textAlignment)
    {
        Array.Fill(_columnAlignments, textAlignment);
    }
    
    /// <summary>
    /// Sets the <see cref="HorizontalTextAlignment"/> of a column
    /// </summary>
    /// <param name="column">Column index</param>
    /// <param name="textAlignment">Text alignment for the column</param>
    public void SetTextAlignment(int column, HorizontalTextAlignment textAlignment)
    {
        _columnAlignments[column] = textAlignment;
    }

    /// <summary>
    /// Renders the table to a string.
    /// </summary>
    /// <param name="escape">If the content of the cells should be escaped. Set to true for correct Markdown. Set to false for pretty console tables.</param>
    public string Render(bool escape = true)
    {
        // 1. calculate column widths. minimum is 3 characters
        var columnWidths = new int[ColumnCount];
        Array.Fill(columnWidths, 3);
        for (var columnIdx = 0; columnIdx < ColumnCount; columnIdx++)
        {
            for (var rowIdx = 0; rowIdx < RowCount; rowIdx++)
            {
                var columnText = _content[rowIdx, columnIdx] ?? string.Empty;
                if (escape)
                {
                    columnText = Escape(columnText);
                }
                
                // spacing to the left and right
                var newWidth = columnText.Length + 2;
                if (newWidth > columnWidths[columnIdx])
                {
                    columnWidths[columnIdx] = newWidth;
                }
            }
        }

        var stringBuilder = new StringBuilder();
        
        // 2. render header row
        stringBuilder.Append('|');
        for (var columnIdx = 0; columnIdx < ColumnCount; columnIdx++)
        {
            var rowIdx = 0;
            
            var columnText = _content[rowIdx, columnIdx] ?? string.Empty;
            if (escape)
            {
                columnText = Escape(columnText);
            }
            
            var columnWidth = columnWidths[columnIdx];

            stringBuilder.Append(' ');
            stringBuilder.Append(_columnAlignments[columnIdx] == HorizontalTextAlignment.Right
                ? columnText.PadLeft(columnWidth - 2)
                : columnText.PadRight(columnWidth - 2));

            stringBuilder.Append(" |");
        }

        stringBuilder.Append(Environment.NewLine);
        
        // 3. render separator
        stringBuilder.Append('|');
        for (var columnIdx = 0; columnIdx < ColumnCount; columnIdx++)
        {
            var columnAlignment = _columnAlignments[columnIdx];
            var columnWidth = columnWidths[columnIdx];

            stringBuilder.Append(SeparatorLeftChar(columnAlignment));
            stringBuilder.Append(new string('-', columnWidth - 2));
            stringBuilder.Append(SeparatorRightChar(columnAlignment));
            stringBuilder.Append('|');
        }

        stringBuilder.Append(Environment.NewLine);
        
        // 4. render table body
        // header was already rendered earlier so start at 1
        for (var rowIdx = 1; rowIdx < RowCount; rowIdx++)
        {
            stringBuilder.Append('|');
            for (var columnIdx = 0; columnIdx < ColumnCount; columnIdx++)
            {
                var columnText = _content[rowIdx, columnIdx] ?? string.Empty;
                if (escape)
                {
                    columnText = Escape(columnText);
                }
                
                var columnWidth = columnWidths[columnIdx];

                stringBuilder.Append(' ');
                stringBuilder.Append(_columnAlignments[columnIdx] == HorizontalTextAlignment.Right
                    ? columnText.PadLeft(columnWidth - 2)
                    : columnText.PadRight(columnWidth - 2));
                stringBuilder.Append(" |");
            }

            stringBuilder.Append(Environment.NewLine);
        }

        return stringBuilder.ToString();
    }

    public override string ToString()
    {
        return Render();
    }

    [return: NotNullIfNotNull(nameof(value))]
    private static string? Escape(string? value)
    {
        if (value == null) return null;

        var stringBuilder = new StringBuilder(value);

        stringBuilder.Replace(@"\", @"\\");
        stringBuilder.Replace(@"*", @"\*");
        stringBuilder.Replace(@"_", @"\_");
        stringBuilder.Replace(@"{", @"\{");
        stringBuilder.Replace(@"}", @"\}");
        stringBuilder.Replace(@"[", @"\[");
        stringBuilder.Replace(@"]", @"\]");
        stringBuilder.Replace(@"<", @"\<");
        stringBuilder.Replace(@">", @"\>");
        stringBuilder.Replace(@"(", @"\(");
        stringBuilder.Replace(@")", @"\)");
        stringBuilder.Replace(@"-", @"\-");
        stringBuilder.Replace(@".", @"\.");
        stringBuilder.Replace(@"|", @"\|");
        stringBuilder.Replace(@"!", @"\!");
        
        return stringBuilder.ToString();
    }

    private static char SeparatorLeftChar(HorizontalTextAlignment alignment)
    {
        return alignment switch
        {
            HorizontalTextAlignment.Left => ':',
            HorizontalTextAlignment.Center => ':',
            HorizontalTextAlignment.Right => '-',
            HorizontalTextAlignment.Default => '-',
            _ => throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null)
        };
    }
    
    private static char SeparatorRightChar(HorizontalTextAlignment alignment)
    {
        return alignment switch
        {
            HorizontalTextAlignment.Left => '-',
            HorizontalTextAlignment.Center => ':',
            HorizontalTextAlignment.Right => ':',
            HorizontalTextAlignment.Default => '-',
            _ => throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null)
        };
    }
}