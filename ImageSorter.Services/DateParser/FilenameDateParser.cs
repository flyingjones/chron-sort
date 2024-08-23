using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ImageSorter.Services.DateParser;

public class FilenameDateParser : IFileNameDateParser
{
    public int Priority { get; }
    private readonly int _yearCaptureGroupIndex;
    private readonly int _monthCaptureGroupIndex;
    private readonly int _dayCaptureGroupIndex;
    private readonly Regex _fileNameRegex;

    public FilenameDateParser(
        [StringSyntax(StringSyntaxAttribute.Regex)]
        string fileNameRegex,
        int priority,
        int yearCaptureGroupIndex = 1,
        int monthCaptureGroupIndex = 2,
        int dayCaptureGroupIndex = 3)
    {
        Priority = priority;
        _yearCaptureGroupIndex = yearCaptureGroupIndex;
        _monthCaptureGroupIndex = monthCaptureGroupIndex;
        _dayCaptureGroupIndex = dayCaptureGroupIndex;
        this._fileNameRegex = new Regex(fileNameRegex);
    }

    public DateTime? ParseDateFromFileName(string fileName)
    {
        var match = _fileNameRegex.Match(fileName);

        if (match.Success)
        {
            var year = int.Parse(match.Groups[_yearCaptureGroupIndex].Value);
            var month = int.Parse(match.Groups[_monthCaptureGroupIndex].Value);
            var day = int.Parse(match.Groups[_dayCaptureGroupIndex].Value);

            return new DateTime(year, month, day);
        }

        return null;
    }
}