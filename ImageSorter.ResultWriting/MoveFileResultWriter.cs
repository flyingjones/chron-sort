using ImageParser.Utils.ProgressLogger;
using ImageSorter.FileWrapper.Abstractions.Directory;
using ImageSorter.FileWrapper.Abstractions.File;
using ImageSorter.ResultWriting.Abstractions.Model;
using Microsoft.Extensions.Logging;

namespace ImageSorter.ResultWriting;

public class MoveFileResultWriter : AbstractResultWriter
{
    private readonly ResultWriterConfig _config;
    private readonly ILogger<MoveFileResultWriter> _logger;
    private readonly IFileWrapper _fileWrapper;
    private readonly IDirectoryWrapper _directoryWrapper;

    public MoveFileResultWriter(
        IDirectoryWrapper directoryWrapper,
        IProgressLogger<MoveFileResultWriter> progressLogger,
        ILogger<MoveFileResultWriter> logger,
        ResultWriterConfig config,
        IFileWrapper fileWrapper)
        : base(directoryWrapper, progressLogger, logger)
    {
        _directoryWrapper = directoryWrapper;
        _logger = logger;
        _config = config;
        _fileWrapper = fileWrapper;
    }

    protected override string LogStartMessageTemplate => "Moving {numberOfFiles} files (this may take a while)";

    protected override Task<FileWriteResultDto> Write(FileWriteDto writeDto, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Moving {Source} to {Destination}", writeDto.SourcePath, writeDto.DestinationPath);

        var result = new FileWriteResultDto
        {
            SourcePath = writeDto.SourcePath,
            DestinationPath = writeDto.DestinationPath,
            Status = FileOperationResultStatus.Error,
            DateTime = writeDto.DateTime
        };
        
        if (writeDto.SourcePath == writeDto.DestinationPath)
        {
            result.Status = FileOperationResultStatus.AlreadyInCorrectPlace;
            return Task.FromResult(result);
        }

        try
        {
            var fileExists = _fileWrapper.Exists(writeDto.DestinationPath);
            if (fileExists && !_config.ShouldOverwrite)
            {
                _logger.Log(_config.LogLevelForSkips,
                    "Skipping {SourcePath} since it already exists at {DestinationPath}",
                    writeDto.SourcePath, writeDto.DestinationPath);
                result.Status = FileOperationResultStatus.Skipped;
                return Task.FromResult(result);
            }
            
            if (fileExists) _logger.LogDebug("Overwriting existing file {DestinationPath}", writeDto.DestinationPath);
            _fileWrapper.Move(writeDto.SourcePath, writeDto.DestinationPath, _config.ShouldOverwrite);
            result.Status = fileExists ? FileOperationResultStatus.OverwriteSuccess : FileOperationResultStatus.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not move file {sourcePath} to {destinationPath}",
                writeDto.SourcePath, writeDto.DestinationPath);
        }

        return Task.FromResult(result);
    }

    protected override void CleanupSourceDirectory()
    {
        DeleteEmptyDirs(_config.SourcePath);
    }
    
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
}