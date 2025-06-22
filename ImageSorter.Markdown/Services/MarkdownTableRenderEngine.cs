using System.Diagnostics.CodeAnalysis;
using System.Text;
using ImageSorter.Markdown.Abstractions.Model;
using ImageSorter.Markdown.Abstractions.Services;

namespace ImageSorter.Markdown.Services;

public class MarkdownTableRenderEngine : IMarkdownTableRenderEngine
{
    /// <summary>
    /// Renders the <paramref name="table"/> to a string.
    /// </summary>
    /// <param name="table">Input table</param>
    /// <param name="escape">If the content of the cells should be escaped. Set to true for correct Markdown. Set to false for pretty console tables.</param>
    public string Render(MarkdownTable table, bool escape = true)
    {
        // 1. calculate column widths. minimum is 3 characters
        var columnWidths = new int[table.ColumnCount];
        Array.Fill(columnWidths, 3);
        for (var columnIdx = 0; columnIdx < table.ColumnCount; columnIdx++)
        {
            for (var rowIdx = 0; rowIdx < table.RowCount; rowIdx++)
            {
                var columnText = table[rowIdx, columnIdx] ?? string.Empty;
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
        for (var columnIdx = 0; columnIdx < table.ColumnCount; columnIdx++)
        {
            var rowIdx = 0;
            
            var columnText = table[rowIdx, columnIdx] ?? string.Empty;
            if (escape)
            {
                columnText = Escape(columnText);
            }
            
            var columnWidth = columnWidths[columnIdx];

            stringBuilder.Append(' ');
            stringBuilder.Append(table.GetTextAlignment(columnIdx) == HorizontalTextAlignment.Right
                ? columnText.PadLeft(columnWidth - 2)
                : columnText.PadRight(columnWidth - 2));

            stringBuilder.Append(" |");
        }

        stringBuilder.Append(Environment.NewLine);
        
        // 3. render separator
        stringBuilder.Append('|');
        for (var columnIdx = 0; columnIdx < table.ColumnCount; columnIdx++)
        {
            var columnAlignment = table.GetTextAlignment(columnIdx);
            var columnWidth = columnWidths[columnIdx];

            stringBuilder.Append(SeparatorLeftChar(columnAlignment));
            stringBuilder.Append(new string('-', columnWidth - 2));
            stringBuilder.Append(SeparatorRightChar(columnAlignment));
            stringBuilder.Append('|');
        }

        stringBuilder.Append(Environment.NewLine);
        
        // 4. render table body
        // header was already rendered earlier so start at 1
        for (var rowIdx = 1; rowIdx < table.RowCount; rowIdx++)
        {
            stringBuilder.Append('|');
            for (var columnIdx = 0; columnIdx < table.ColumnCount; columnIdx++)
            {
                var columnText = table[rowIdx, columnIdx] ?? string.Empty;
                if (escape)
                {
                    columnText = Escape(columnText);
                }
                
                var columnWidth = columnWidths[columnIdx];

                stringBuilder.Append(' ');
                stringBuilder.Append(table.GetTextAlignment(columnIdx) == HorizontalTextAlignment.Right
                    ? columnText.PadLeft(columnWidth - 2)
                    : columnText.PadRight(columnWidth - 2));
                stringBuilder.Append(" |");
            }

            stringBuilder.Append(Environment.NewLine);
        }

        return stringBuilder.ToString();
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