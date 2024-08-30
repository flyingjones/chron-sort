using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ImageSorter.Services.DateParser;

public class FilenameDateParser : IFileNameDateParser
{
    public int Priority { get; }
    private readonly DateTime _ignoreAfter;
    private const string YearCaptureGroupName = "year";
    private const string MonthCaptureGroupName = "month";
    private const string DayCaptureGroupName = "day";
    private readonly Regex _fileNameRegex;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileNameRegex">
    /// Regex to parse a file name with.
    /// Expects a regex with exactly three named capture groups with the names <c>year</c>, <c>month</c>, <c>day</c>.
    /// </param>
    /// <param name="priority">The priority of this parser</param>
    /// <param name="ignoreAfter">TODO remove me and expose this as a general setting</param>
    public FilenameDateParser(
        [StringSyntax(StringSyntaxAttribute.Regex)]
        string fileNameRegex,
        int priority,
        DateTime ignoreAfter)
    {
        Priority = priority;
        _ignoreAfter = ignoreAfter;
        _fileNameRegex = new Regex(fileNameRegex);

        ThrowIfRegexInvalid(_fileNameRegex);
    }

    public bool TryParseDateFromFileName(string fileName, [NotNullWhen(true)] out DateTime? parsedDate)
    {
        var match = _fileNameRegex.Match(fileName);
        parsedDate = null;

        if (!match.Success) return false;

        var year = int.Parse(match.Groups[YearCaptureGroupName].Value);
        var month = int.Parse(match.Groups[MonthCaptureGroupName].Value);
        var day = int.Parse(match.Groups[DayCaptureGroupName].Value);

        var result = new DateTime(year, month, day);

        if (result <= _ignoreAfter)
        {
            parsedDate = result;
            return true;
        }

        return false;
    }

    private static void ThrowIfRegexInvalid(Regex fileNameRegex)
    {
        var namedCaptureGroups = fileNameRegex.GetGroupNames();
        
        if (namedCaptureGroups.Length != 4)
        {
            throw new ArgumentException("Should contain three named capture groups", nameof(fileNameRegex));
        }

        if (!namedCaptureGroups.Contains(YearCaptureGroupName))
        {
            throw new ArgumentException($"Missing named capture group \"{YearCaptureGroupName}\"", nameof(fileNameRegex));
        }
        
        if (!namedCaptureGroups.Contains(MonthCaptureGroupName))
        {
            throw new ArgumentException($"Missing named capture group \"{MonthCaptureGroupName}\"", nameof(fileNameRegex));
        }
        
        if (!namedCaptureGroups.Contains(DayCaptureGroupName))
        {
            throw new ArgumentException($"Missing named capture group \"{DayCaptureGroupName}\"", nameof(fileNameRegex));
        }
    }
}