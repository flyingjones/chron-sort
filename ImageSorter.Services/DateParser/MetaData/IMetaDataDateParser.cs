namespace ImageSorter.Services.DateParser.MetaData;

public interface IMetaDataDateParser
{
    /// <summary>
    /// Tries to get a date time from the meta data of an image / video etc.
    /// </summary>
    Task<DateTime?> ParseDate(string filePath);
}