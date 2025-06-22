using System.Diagnostics.CodeAnalysis;
using ImageSorter.DateParsing.Abstractions.Services.MetaData;

namespace ImageSorter.DateParsing.Abstractions.Services;

public interface IDateParserImplementation
{
    bool TryParseDate(ILazyFileMetaDataHandle fileHandle, [NotNullWhen(true)] out DateTime? result);
    
    int Priority { get; }
    
    string Name { get; }
}