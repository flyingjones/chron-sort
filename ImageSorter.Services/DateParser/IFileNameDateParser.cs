using System.Diagnostics.CodeAnalysis;

namespace ImageSorter.Services.DateParser;

public interface IFileNameDateParser
{
    bool TryParseDateFromFileName(string fileName, [NotNullWhen(true)] out DateTime? parsedDate);
    
    int Priority { get; }
}