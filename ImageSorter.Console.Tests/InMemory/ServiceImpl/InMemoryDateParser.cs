using ImageSorter.DateParsing.Abstractions.Services;

namespace ImageSorter.Tests.InMemory.ServiceImpl;

public class InMemoryDateParser : IDateParser
{
    private readonly InMemoryFileSystem _inMemoryFileSystem;

    public InMemoryDateParser(InMemoryFileSystem inMemoryFileSystem)
    {
        _inMemoryFileSystem = inMemoryFileSystem;
    }
    
    public DateTime ParseDate(string filePath, out string parserName)
    {
        var file = _inMemoryFileSystem.GetFile(filePath);
        if (file == null)
        {
            throw new FileNotFoundException($"file {filePath} not found");
        }

        parserName = "inMemoryMock";
        return file.DateTime;
    }
}