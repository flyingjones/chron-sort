using System.Diagnostics.CodeAnalysis;

namespace ImageSorter.Services.DateParser.MetaData;

public interface IMetaDataDateParser
{
    bool TryParseDate(FileMetaDataHandle fileMetaDataHandle, [NotNullWhen(true)] out DateTime? result);
    
    int Priority { get; }
}