namespace ImageSorter.Services.DateParser;

public class DateParserConfiguration
{
    public required DateTime SkipParserBefore { get; set; }
    
    public required DateTime SkipParserAfter { get; set; }
}