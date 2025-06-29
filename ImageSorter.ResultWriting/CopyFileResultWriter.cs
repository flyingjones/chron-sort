using ImageParser.Utils.ProgressLogger;
using ImageSorter.FileWrapper.Abstractions.Directory;
using ImageSorter.FileWrapper.Abstractions.File;
using ImageSorter.FileWrapper.Abstractions.FileStream;
using ImageSorter.ResultWriting.Abstractions;
using ImageSorter.ResultWriting.Abstractions.Model;
using Microsoft.Extensions.Logging;

namespace ImageSorter.ResultWriting;

public class CopyFileResultWriter : AbstractResultWriter
{
    private readonly ILogger<CopyFileResultWriter> _logger;
    private readonly ResultWriterConfig _config;
    private readonly IFileWrapper _fileWrapper;
    private readonly IFileStreamService _fileStreamService;

    public CopyFileResultWriter(
        IDirectoryWrapper directoryWrapper,
        ILogger<CopyFileResultWriter> logger,
        ResultWriterConfig config,
        IFileWrapper fileWrapper,
        IFileStreamService fileStreamService,
        IProgressLogger<CopyFileResultWriter> progressLogger) 
        : base(directoryWrapper, progressLogger, logger)
    {
        _logger = logger;
        _config = config;
        _fileWrapper = fileWrapper;
        _fileStreamService = fileStreamService;
    }
    protected override string LogStartMessageTemplate => "Copying {numberOfFiles} files (this may take a while)";

    protected override async Task<FileWriteResultDto> Write(FileWriteDto writeDto, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Copying {Source} to {Destination}", writeDto.SourcePath, writeDto.DestinationPath);
        
        var result = new FileWriteResultDto
        {
            SourcePath = writeDto.SourcePath,
            DestinationPath = writeDto.DestinationPath,
            Status = FileOperationResultStatus.Error,
            DateTime = writeDto.DateTime
        };

        try
        {
            var fileExists = _fileWrapper.Exists(writeDto.DestinationPath);
            if (fileExists && !_config.ShouldOverwrite)
            {
                _logger.Log(_config.LogLevelForSkips, "Skipping {SourcePath} since it already exists at {DestinationPath}",
                    writeDto.SourcePath, writeDto.DestinationPath);
                result.Status = FileOperationResultStatus.Skipped;
                return result;
            }

            if (fileExists) _logger.LogDebug("Overwriting existing file {DestinationPath}", writeDto.DestinationPath);
            await _fileStreamService.CopyToAsync(writeDto.SourcePath, writeDto.DestinationPath, cancellationToken);
            result.Status = fileExists ? FileOperationResultStatus.OverwriteSuccess : FileOperationResultStatus.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not copy file {sourcePath} to {destinationPath}",
                writeDto.SourcePath, writeDto.DestinationPath);
        }

        return result;
    }
}