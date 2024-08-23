namespace ImageSorter.Services.DateParser;

public interface IFileNameDateParser
{
    DateTime? ParseDateFromFileName(string fileName);
    
    int Priority { get; }
}