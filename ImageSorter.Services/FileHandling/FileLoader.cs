namespace ImageSorter.Services.FileHandling;

public class FileLoader : IFileLoader
{
    private readonly FileLoaderOptions _options;

    public FileLoader(FileLoaderOptions options)
    {
        _options = options;
    }

    public string[] GetFilePaths()
    {
        var allFiles = Directory.GetFiles(_options.SourcePath, "*", searchOption: SearchOption.AllDirectories);

        return FilterFilePathsByFileEnding(allFiles);
    }

    private string[] FilterFilePathsByFileEnding(string[] filePaths)
    {
        if (_options.FileEndings?.Length > 0)
        {
            return filePaths.Where(x => _options.FileEndings.Contains(FileEnding(x))).ToArray();
        }

        return filePaths;
    }
    
    private static string FileEnding(string filePath)
    {
        return filePath.Split(".")[^1];
    }
}