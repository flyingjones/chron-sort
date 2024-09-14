using ImageSorter.Services.DateParser;
using ImageSorter.Services.FileHandling;
using Microsoft.Extensions.Logging;

namespace ImageSorter;

public partial class Sorter : ISorter
{
    private readonly ILogger<Sorter> _logger;
    private readonly IFileLoader _fileLoader;
    private readonly IDateParsingHandler _dateParsingHandler;
    private readonly IDestinationWriter _destinationWriter;

    public Sorter(
        ILogger<Sorter> logger,
        IFileLoader fileLoader,
        IDateParsingHandler dateParsingHandler,
        IDestinationWriter destinationWriter)
    {
        _logger = logger;
        _fileLoader = fileLoader;
        _dateParsingHandler = dateParsingHandler;
        _destinationWriter = destinationWriter;
    }

    public async Task PerformSorting(bool moveFiles, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scanning source");
        var filesToProcess = _fileLoader.GetFilePaths();

        var writeQueue = await _dateParsingHandler.ScanFiles(filesToProcess, cancellationToken);

        if (!moveFiles)
        {
            await _destinationWriter.CopyFiles(writeQueue, cancellationToken);
        }
        else
        {
            _destinationWriter.MoveFiles(writeQueue, cancellationToken);
        }

        _logger.LogInformation("Finished sorting");
    }
}