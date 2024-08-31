namespace ImageSorter.Services.DateParser;

public interface IDateParser
{
    /// <summary>
    /// Uses all registered instances of the <see cref="IDateParserImplementation"/> to find a date for a file path
    /// </summary>
    DateTime ParseDate(string filePath);
}