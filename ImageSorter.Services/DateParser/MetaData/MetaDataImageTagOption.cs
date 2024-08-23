namespace ImageSorter.Services.DateParser.MetaData;

public class MetaDataImageTagOption
{
    public required int TagId { get; init; }
    
    public required Func<string, DateTime?> ParserFund { get; init; }
}