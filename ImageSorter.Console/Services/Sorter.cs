using ImageSorter.DateParsing.Abstractions.Services;
using ImageSorter.Services.FileHandling;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Services;

public partial class Sorter : ISorter
{
    private readonly ILogger<Sorter> _logger;
    private readonly IFileLoader _fileLoader;
    private readonly IDateParsingHandler _dateParsingHandler;
    private readonly IDestinationWriter _destinationWriter;
    private readonly ISummaryReportingService _summaryReportingService;

    public Sorter(
        ILogger<Sorter> logger,
        IFileLoader fileLoader,
        IDateParsingHandler dateParsingHandler,
        IDestinationWriter destinationWriter,
        ISummaryReportingService summaryReportingService)
    {
        _logger = logger;
        _fileLoader = fileLoader;
        _dateParsingHandler = dateParsingHandler;
        _destinationWriter = destinationWriter;
        _summaryReportingService = summaryReportingService;
    }

    public async Task PerformSorting(bool moveFiles, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scanning Source Directory");
        var filesToProcess = _fileLoader.GetFilePaths(out var allFiles);
        _summaryReportingService.ReportScanSummary(filesToProcess, allFiles);

        _logger.LogInformation("Parsing Dates");
        var writeQueue = (await _dateParsingHandler.ScanFiles(filesToProcess, cancellationToken)).ToList();
        _summaryReportingService.ReportSortSummary(writeQueue);

        _logger.LogInformation("Writing results");
        ICollection<FileOperationResult> writeSummary;
        if (!moveFiles)
        {
            writeSummary = await _destinationWriter.CopyFiles(writeQueue, cancellationToken);
        }
        else
        {
            writeSummary = _destinationWriter.MoveFiles(writeQueue, cancellationToken);
        }

        _summaryReportingService.ReportWriteSummary(writeSummary);

        _logger.LogInformation("Finished");
    }
}