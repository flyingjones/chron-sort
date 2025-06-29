using ImageParser.Utils.ProgressLogger;
using ImageSorter.FileWrapper.Abstractions.Directory;
using ImageSorter.ResultWriting.Abstractions;
using ImageSorter.ResultWriting.Abstractions.Model;
using Microsoft.Extensions.Logging;

namespace ImageSorter.ResultWriting;

public abstract class AbstractResultWriter : IResultWriter
{
    private readonly IDirectoryWrapper _directoryWrapper;
    private readonly IProgressLogger _progressLogger;
    private readonly ILogger _logger;

    protected AbstractResultWriter(
        IDirectoryWrapper directoryWrapper,
        IProgressLogger progressLogger,
        ILogger logger)
    {
        _directoryWrapper = directoryWrapper;
        _progressLogger = progressLogger;
        _logger = logger;
    }

    public async Task<ICollection<FileWriteResultDto>> Write(ICollection<FileWriteDto> writeDtos, CancellationToken cancellationToken)
    {
        // group writes by directory so we don't have to call the os as often
        var groupedWrites = writeDtos.GroupBy(x => _directoryWrapper.GetParentDirectory(x.DestinationPath));

        var fileCount = writeDtos.Count;
        _progressLogger.LogStart(LogStartMessageTemplate, fileCount);
        var currentIndex = 0;
        var result = new List<FileWriteResultDto>();
        
        foreach (var group in groupedWrites)
        {
            if (group.Key == null)
            {
                _logger.LogWarning("Encountered null directory");
                continue;
            }

            _directoryWrapper.CreateDirectory(group.Key);

            foreach (var writeDto in group)
            {

                var writeResult = await Write(writeDto, cancellationToken);
                result.Add(writeResult);
                
                _progressLogger.LogProgress((double) currentIndex / fileCount);
                currentIndex++;
            }
        }
        
        _progressLogger.LogProgressFinished();
        
        CleanupSourceDirectory();

        return result;
    }

    protected abstract string LogStartMessageTemplate { get; }

    protected abstract Task<FileWriteResultDto> Write(FileWriteDto writeDto, CancellationToken cancellationToken);

    protected virtual void CleanupSourceDirectory()
    {
        // no op
    }
}