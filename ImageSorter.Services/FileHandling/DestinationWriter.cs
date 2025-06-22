using System.Text;
using ImageParser.Utils.DateTimeWrapper;
using ImageParser.Utils.ProgressLogger;
using ImageSorter.DateParsing.Abstractions.Model.MetaData;
using ImageSorter.FileHandling.Directory;
using ImageSorter.FileHandling.File;
using ImageSorter.FileHandling.FileStream;
using ImageSorter.FileWrapper.Abstractions.Directory;
using ImageSorter.FileWrapper.Abstractions.File;
using ImageSorter.FileWrapper.Abstractions.FileStream;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Services.FileHandling;

/// <inheritdoc cref="IDestinationWriter"/>
public partial class DestinationWriter : IDestinationWriter
{
    private readonly DestinationWriterOptions _options;
    private readonly ILogger<DestinationWriter> _logger;
    private readonly IFileWrapper _fileWrapper;
    private readonly IDirectoryWrapper _directoryWrapper;
    private readonly IFileStreamService _fileStreamService;
    private readonly IDateDirectory _dateDirectory;
    private readonly IProgressLogger<DestinationWriter> _progressLogger;
    private readonly IBufferedStreamWriterFactory _bufferedStreamWriterFactory;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DestinationWriter(
        DestinationWriterOptions options,
        ILogger<DestinationWriter> logger,
        IFileWrapper fileWrapper,
        IDirectoryWrapper directoryWrapper,
        IFileStreamService fileStreamService,
        IDateDirectory dateDirectory,
        IProgressLogger<DestinationWriter> progressLogger,
        IBufferedStreamWriterFactory bufferedStreamWriterFactory,
        IDateTimeProvider dateTimeProvider)
    {
        _options = options;
        _logger = logger;
        _fileWrapper = fileWrapper;
        _directoryWrapper = directoryWrapper;
        _fileStreamService = fileStreamService;
        _dateDirectory = dateDirectory;
        _progressLogger = progressLogger;
        _bufferedStreamWriterFactory = bufferedStreamWriterFactory;
        _dateTimeProvider = dateTimeProvider;
        _directoryWrapper.CreateDirectory(options.DestinationPath);
    }

    public async Task<FileOperationResult> CopyFile(string sourcePath, DateTime dateTime,
        CancellationToken cancellationToken)
    {
        var monthPath = _dateDirectory.CreatePathAndDirs(dateTime);
        var fileName = Path.GetFileName(sourcePath);
        var destinationPath = Path.GetFullPath($"{monthPath}/{fileName}");
        var result = new FileOperationResult
        {
            SourcePath = sourcePath,
            DestinationPath = destinationPath,
            Status = FileOperationResultStatus.Error,
            FileDate = dateTime
        };

        LogWriting("Copying", sourcePath, destinationPath);
        try
        {
            var fileExists = _fileWrapper.Exists(destinationPath);
            if (fileExists && !_options.OverwriteExistingFiles)
            {
                LogSkip();
                result.Status = FileOperationResultStatus.Skipped;
                return result;
            }

            if (fileExists) LogOverwrite();
            await _fileStreamService.CopyToAsync(sourcePath, destinationPath, cancellationToken);
            result.Status = fileExists ? FileOperationResultStatus.OverwriteSuccess : FileOperationResultStatus.Success;
        }
        catch (Exception ex)
        {
            LogError(ex, sourcePath, destinationPath);
        }

        return result;
    }

    public FileOperationResult MoveFile(string sourcePath, DateTime dateTime)
    {
        var monthPath = _dateDirectory.CreatePathAndDirs(dateTime);
        var fileName = Path.GetFileName(sourcePath);
        var destinationPath = Path.GetFullPath($"{monthPath}/{fileName}");
        var result = new FileOperationResult
        {
            SourcePath = sourcePath,
            DestinationPath = destinationPath,
            Status = FileOperationResultStatus.Error,
            FileDate = dateTime
        };

        LogWriting("Moving", sourcePath, destinationPath);
        // the file is now at the desired location -> return true
        if (sourcePath == destinationPath)
        {
            result.Status = FileOperationResultStatus.AlreadyInCorrectPlace;
            return result;
        }

        try
        {
            var fileExists = _fileWrapper.Exists(destinationPath);
            if (fileExists && !_options.OverwriteExistingFiles)
            {
                LogSkip();
                result.Status = FileOperationResultStatus.Skipped;
                return result;
            }

            if (fileExists) LogOverwrite();
            _fileWrapper.Move(sourcePath, destinationPath, _options.OverwriteExistingFiles);
            result.Status = fileExists ? FileOperationResultStatus.OverwriteSuccess : FileOperationResultStatus.Success;
        }
        catch (Exception ex)
        {
            LogError(ex, sourcePath, destinationPath);
        }

        return result;
    }

    /// <inheritdoc cref="IDestinationWriter.CopyFiles"/>
    public async Task<ICollection<FileOperationResult>> CopyFiles(ICollection<ParsedFileResult> parsedFileResults,
        CancellationToken cancellationToken)
    {
        var count = parsedFileResults.Count;

        var copySummary = new List<FileOperationResult>();

        _progressLogger.LogStart("Copying {count} files (this may take a while)", count);

        var idx = 0;

        foreach (var item in parsedFileResults)
        {
            _progressLogger.LogProgress((double)idx / count);

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogError("Execution Canceled");
                return copySummary;
            }

            var copyResult = await CopyFile(item.FilePath, item.DateTaken, cancellationToken);
            copyResult.ParserName = item.ParserName;
            copySummary.Add(copyResult);
            ++idx;
        }

        _progressLogger.LogProgressFinished();

        return copySummary;
    }

    /// <inheritdoc cref="IDestinationWriter.MoveFiles"/>
    public ICollection<FileOperationResult> MoveFiles(ICollection<ParsedFileResult> parsedFileResults,
        CancellationToken cancellationToken)
    {
        var count = parsedFileResults.Count;

        var moveSummary = new List<FileOperationResult>();

        _progressLogger.LogStart("Moving {count} files (this may take a while)", count);

        var idx = 0;

        foreach (var item in parsedFileResults)
        {
            _progressLogger.LogProgress((double)idx / count);

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogError("Execution Canceled");
                return moveSummary;
            }

            var moveResult = MoveFile(item.FilePath, item.DateTaken);
            moveResult.ParserName = item.ParserName;
            ++idx;
            moveSummary.Add(moveResult);
        }

        _progressLogger.LogProgressFinished();

        DeleteEmptyDirs(_options.SourcePath);
        return moveSummary;
    }

    /// <summary>
    /// Delete all empty subdirectories
    /// </summary>
    private void DeleteEmptyDirs(string path)
    {
        try
        {
            foreach (var d in _directoryWrapper.EnumerateDirectories(path))
            {
                DeleteEmptyDirs(d);
            }

            var entries = _directoryWrapper.EnumerateFileSystemEntries(path);

            if (!entries.Any())
            {
                try
                {
                    _directoryWrapper.Delete(path);
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (DirectoryNotFoundException)
                {
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    [LoggerMessage(Level = LogLevel.Trace, Message = "Skipping file since it already exists at the destination")]
    private partial void LogSkip();

    [LoggerMessage(Level = LogLevel.Trace, Message = "Overwriting existing file")]
    private partial void LogOverwrite();

    [LoggerMessage(Level = LogLevel.Trace, Message = "{operation} {sourcePath} to {destinationPath}")]
    private partial void LogWriting(string operation, string sourcePath, string destinationPath);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Could not write file {sourcePath} to {destinationPath}")]
    private partial void LogError(Exception exception, string sourcePath, string destinationPath);

    [LoggerMessage(Level = LogLevel.Information, Message = "Sorting Summary {summary}")]
    private partial void LogSummaryMessage(string summary);
}