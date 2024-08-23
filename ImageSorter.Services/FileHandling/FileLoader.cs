using ImageSorter.Services.FileWrapper;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Services.FileHandling;

public partial class FileLoader : IFileLoader
{
    private readonly FileLoaderOptions _options;
    private readonly IDirectoryWrapper _directoryWrapper;
    private readonly ILogger<FileLoader> _logger;

    public FileLoader(FileLoaderOptions options, IDirectoryWrapper directoryWrapper, ILogger<FileLoader> logger)
    {
        _options = options;
        _directoryWrapper = directoryWrapper;
        _logger = logger;
    }

    /// <inheritdoc cref="IFileLoader.GetFilePaths"/>
    public string[] GetFilePaths()
    {
        var allFiles = _directoryWrapper.GetFiles(_options.SourcePath, "*", searchOption: SearchOption.AllDirectories);

        var result = FilterFilePathsByFileEnding(allFiles);
        LogFileCount(result.Length);
        return result;
    }

    private string[] FilterFilePathsByFileEnding(string[] filePaths)
    {
        if (_options.FileEndings?.Length > 0)
        {
            return filePaths.Where(x => _options.FileEndings.Contains(FileEnding(x))).ToArray();
        }

        return filePaths;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Found {fileCount} files to sort")]
    private partial void LogFileCount(int fileCount);
    
    private static string FileEnding(string filePath)
    {
        return filePath.Split(".")[^1];
    }
}