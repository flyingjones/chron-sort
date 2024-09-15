using System.Diagnostics.CodeAnalysis;

namespace ImageSorter.Services.DateParser.MetaData;

public abstract class MetaDataParserBase : IDateParserImplementation
{
    protected MetaDataParserBase(int priority)
    {
        Priority = priority;
    }

    public int Priority { get; }
    public abstract string Name { get; }

    protected abstract IEnumerable<string> SupportedFileEndings();

    private bool FileEndingSupported(string fileEnding)
    {
        return SupportedFileEndings().Contains(fileEnding, StringComparer.InvariantCultureIgnoreCase);
    }

    public bool TryParseDate(ILazyFileMetaDataHandle fileHandle, [NotNullWhen(true)] out DateTime? result)
    {
        result = null;
        if (!FileEndingSupported(fileHandle.FileEnding))
            return false;

        var metaDataWrapper = fileHandle.GetOrLoadTagWrapper();
        
        return metaDataWrapper.HasMetaData && TryParseMetaDataDirectories(metaDataWrapper, out result);
    }

    protected abstract bool TryParseMetaDataDirectories(
        IMetaDataTagWrapper metaDataWrapper,
        [NotNullWhen(true)] out DateTime? result);
}