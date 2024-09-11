using ImageSorter.Services.DateTimeWrapper;
using ImageSorter.Services.FileWrapper;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Services.FileHandling;

/// <summary>
/// Will not actually move any files but write the queue to a txt file
/// </summary>
public partial class DryRunDestinationWriter : IDestinationWriter
{
    private readonly DestinationWriterOptions _options;
    private readonly ILogger<DryRunDestinationWriter> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IFileWrapper _fileWrapper;
    private readonly IDirectoryWrapper _directoryWrapper;
    private readonly IBufferedStreamWriterFactory _bufferedStreamWriterFactory;

    public DryRunDestinationWriter(
        DestinationWriterOptions options,
        ILogger<DryRunDestinationWriter> logger,
        IDateTimeProvider dateTimeProvider,
        IFileWrapper fileWrapper,
        IDirectoryWrapper directoryWrapper,
        IBufferedStreamWriterFactory bufferedStreamWriterFactory)
    {
        _options = options;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        _fileWrapper = fileWrapper;
        _directoryWrapper = directoryWrapper;
        _bufferedStreamWriterFactory = bufferedStreamWriterFactory;
    }

    public Task CopyFiles(IEnumerable<WriteQueueItem> writeQueueItems, CancellationToken cancellationToken)
    {
        return Task.Run(() => { WriteQueueToFile("Copy", writeQueueItems, cancellationToken); }, cancellationToken);
    }

    public void MoveFiles(IEnumerable<WriteQueueItem> writeQueueItems, CancellationToken cancellationToken)
    {
        WriteQueueToFile("Move", writeQueueItems, cancellationToken);
    }

    private void WriteQueueToFile(string operation, IEnumerable<WriteQueueItem> writeQueueItems,
        CancellationToken cancellationToken)
    {
        var targetFileName = BuiltDryRunFileName();

        _directoryWrapper.CreateDirectory(_options.DestinationPath);

        var targetFilePath = Path.GetFullPath($"{_options.DestinationPath}/{targetFileName}");

        LogOutputFile(targetFilePath);

        using var textWriter = _bufferedStreamWriterFactory.CreateStreamWriter(targetFilePath, FileMode.CreateNew);

        var yearGroupings = writeQueueItems
            .GroupBy(x => x.DateTaken.Year)
            .OrderBy(x => x.Key);

        foreach (var yearGrouping in yearGroupings)
        {
            var year = yearGrouping.Key;
            var monthGroupings = yearGrouping
                .GroupBy(x => x.DateTaken.Month)
                .OrderBy(x => x.Key);

            foreach (var monthGrouping in monthGroupings)
            {
                var month = monthGrouping.Key;

                var currentPath = Path.GetFullPath($"{_options.DestinationPath}/{year:0000}/{month:00}");
                textWriter.WriteLine(currentPath);

                foreach (var item in monthGrouping.OrderBy(x => Path.GetFileName(x.FilePath)))
                {
                    var fileName = Path.GetFileName(item.FilePath);
                    var targetPath = Path.GetFullPath($"{currentPath}/{fileName}");

                    var fileExists = _fileWrapper.Exists(targetPath);
                    textWriter.WriteLine($"    {BuiltOperationString(operation, fileExists, item)}");
                }
            }
        }
    }

    [LoggerMessage(LogLevel.Information, Message = "Writing sorting result to {filePath}")]
    private partial void LogOutputFile(string filePath);

    private string BuiltOperationString(string operation, bool fileExists, WriteQueueItem queueItem)
    {
        var infoString = (fileExists, _options.OverwriteExistingFiles) switch
        {
            (false, _) => string.Empty,
            (true, false) => "[SkipExisting] ",
            (true, true) => "[OverwriteExisting] "
        };
        return $"[{operation}] {infoString}{queueItem.FilePath} [{queueItem.DateTaken:o}]";
    }

    private string BuiltDryRunFileName()
    {
        return $"sort_{_dateTimeProvider.Now():yyyy-MM-dd_hh_mm_ss}.txt";
    }
}