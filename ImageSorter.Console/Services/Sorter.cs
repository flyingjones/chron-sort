using System.Reflection;
using ImageSorter.DateParsing.Abstractions.Services;
using ImageSorter.DependencyInjection;
using ImageSorter.Services.FileHandling;
using ImageSorter.Sorting.Abstractions.Model;
using ImageSorter.Sorting.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Services;

public partial class Sorter : ISorter
{
    private readonly SortRunConfiguration _sortRunConfiguration;
    private readonly ILogger<Sorter> _logger;
    private readonly IFileLoader _fileLoader;
    private readonly IDateParsingHandler _dateParsingHandler;
    private readonly IDestinationWriter _destinationWriter;
    private readonly ISummaryReportingService _summaryReportingService;
    private readonly Sorting.Abstractions.Services.ISorter _newSorter;
    private readonly IConflictFinder _conflictFinder;
    private readonly IConflictReducer _conflictReducer;

    public Sorter(
        ILogger<Sorter> logger,
        IFileLoader fileLoader,
        IDateParsingHandler dateParsingHandler,
        IDestinationWriter destinationWriter,
        ISummaryReportingService summaryReportingService,
        Sorting.Abstractions.Services.ISorter newSorter,
        IConflictFinder conflictFinder, IConflictReducer conflictReducer, SortRunConfiguration sortRunConfiguration)
    {
        _logger = logger;
        _fileLoader = fileLoader;
        _dateParsingHandler = dateParsingHandler;
        _destinationWriter = destinationWriter;
        _summaryReportingService = summaryReportingService;
        _newSorter = newSorter;
        _conflictFinder = conflictFinder;
        _conflictReducer = conflictReducer;
        _sortRunConfiguration = sortRunConfiguration;
    }

    public async Task PerformSorting(bool moveFiles, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scanning Source Directory");
        var filesToProcess = _fileLoader.GetFilePaths(_sortRunConfiguration.SourcePath, out var allFiles);
        _summaryReportingService.ReportScanSummary(filesToProcess, allFiles);

        _logger.LogInformation("Parsing Dates");
        var parsedFiles = (await _dateParsingHandler.ScanFiles(filesToProcess, cancellationToken))
            .Select(x => new FilePathWithParsedDate
            {
                FilePath = x.FilePath,
                DateTime = x.DateTaken
            })
            .ToArray();

        _logger.LogInformation("Sorting Files");
        var rawSorting = _newSorter.SortFiles(parsedFiles);
        // _summaryReportingService.ReportSortSummary(rawSorting);

        string[] filesAtDestination = [];
        if (_sortRunConfiguration is { IsInPlace: false, DestinationConflictMode: DestinationConflictMode.Joint })
        {
            _logger.LogInformation("Scanning Destination Directory");
            filesAtDestination = _fileLoader.GetFilePaths(_sortRunConfiguration.DestinationPath, out var _);
        }
        
        _logger.LogInformation("Searching for Conflicts");
        var conflicts = _conflictFinder.FindConflicts(rawSorting, filesAtDestination);

        _logger.LogInformation("Performing Conflict Reduction");
        var reducedConflicts = _conflictReducer.ReduceConflicts(conflicts);
        
        _summaryReportingService.ReportConflictSummary(reducedConflicts);

        // files to write after sorting and conflict reduction
        var writeTarget = reducedConflicts.NonConflictingFiles
            .Concat(reducedConflicts.ReducedSortingConflicts.SelectMany(x => x.ChosenFiles))
            .ToArray();

        // remaining conflicts after reduction
        var sortConflictSummaryAfterReduction = new SortingConflictSummary
        {
            NonConflictingFiles = reducedConflicts.NonConflictingFiles
                .Concat(reducedConflicts.ReducedSortingConflicts
                    .Where(x => x.IsResolved)
                    .SelectMany(x => x.ChosenFiles))
                .ToArray(),
            Conflicts = reducedConflicts.ReducedSortingConflicts
                .Where(x => !x.IsResolved)
                .Select(x => new SortingConflict
                {
                    ConflictingFiles = x.ChosenFiles
                        .Select(chosenFile => new FilePathWrapper(chosenFile))
                        .Concat(x.ConflictingFiles.Where(y => !y.IsFromSource))
                        .ToArray()
                })
                .ToArray()
        };
        
        // TBD implement conflict resolution (Throw / ChooseOne / SemanticRename / RandomRename)
        
        // TBD implement writing of results, pass overwrite true only if DestinationConflict is set to Overwrite
        
        // TBD        
        
        _logger.LogInformation("blub ({Sas})", sortConflictSummaryAfterReduction);

        // _summaryReportingService.ReportSortSummary(writeQueue);
        //
        // _logger.LogInformation("Writing results");
        // ICollection<FileOperationResult> writeSummary;
        // if (!moveFiles)
        // {
        //     writeSummary = await _destinationWriter.CopyFiles(writeQueue, cancellationToken);
        // }
        // else
        // {
        //     writeSummary = _destinationWriter.MoveFiles(writeQueue, cancellationToken);
        // }
        //
        // _summaryReportingService.ReportWriteSummary(writeSummary);
        //
        // _logger.LogInformation("Finished");
    }
}