namespace ImageSorter.DateParsing.Abstractions.Model.MetaData;

public class ParsedFileResult
{
    public required string FilePath { get; set; }
    
    public required DateTime DateTaken { get; set; }
    
    public string? ParserName { get; set; }
}