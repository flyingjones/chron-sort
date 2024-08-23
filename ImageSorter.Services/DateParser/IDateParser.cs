namespace ImageSorter.Services.DateParser;

public interface IDateParser
{
    Task<DateTime> ParseDate(string filePath);
}