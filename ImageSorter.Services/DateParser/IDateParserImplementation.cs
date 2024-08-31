using System.Diagnostics.CodeAnalysis;
using ImageSorter.Services.DateParser.MetaData;

namespace ImageSorter.Services.DateParser;

public interface IDateParserImplementation
{
    bool TryParseDate(ILazyFileMetaDataHandle fileHandle, [NotNullWhen(true)] out DateTime? result);
    
    int Priority { get; }
    
    string Name { get; }
}