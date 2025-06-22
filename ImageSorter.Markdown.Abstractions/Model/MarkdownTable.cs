using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ImageSorter.Markdown.Abstractions.Model;

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
}