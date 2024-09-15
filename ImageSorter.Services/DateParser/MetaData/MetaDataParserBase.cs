using System.Diagnostics.CodeAnalysis;
using Directory = MetadataExtractor.Directory;

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

        var directories = fileHandle.GetOrLoadMetaDataDirectories();
        if (directories is null)
            return false;

        return TryParseMetaDataDirectories(directories, out result);
    }

    protected abstract bool TryParseMetaDataDirectories(IReadOnlyList<Directory> directories,
        [NotNullWhen(true)] out DateTime? result);
}