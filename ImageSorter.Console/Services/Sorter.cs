using System.Reflection;
using ImageSorter.DateParsing.Abstractions.Services;
using ImageSorter.DependencyInjection;
using ImageSorter.Services.FileHandling;
using ImageSorter.Sorting.Abstractions.Model;
using ImageSorter.Sorting.Abstractions.Services;
using ImageSorter.Sorting.Model;
using ImageSorter.Sorting.Services;
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
    private readonly IConflictResolver _conflictResolver;

    public Sorter(
        ILogger<Sorter> logger,
        IFileLoader fileLoader,
        IDateParsingHandler dateParsingHandler,
        IDestinationWriter destinationWriter,
        ISummaryReportingService summaryReportingService,
        Sorting.Abstractions.Services.ISorter newSorter,
        IConflictFinder conflictFinder,
        IConflictReducer conflictReducer,
        SortRunConfiguration sortRunConfiguration,
        IConflictResolver conflictResolver)
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
        _conflictResolver = conflictResolver;
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

        _logger.LogInformation("Scanning Destination Directory");
        string[] filesAtDestination = _fileLoader.GetFilePaths(_sortRunConfiguration.DestinationPath, out var _);

        _logger.LogInformation("Searching for Conflicts");
        var conflicts = _conflictFinder.FindConflicts(rawSorting, filesAtDestination);

        _logger.LogInformation("Performing Conflict Reduction");
        var reducedConflicts = _conflictReducer.ReduceConflicts(conflicts);

        _summaryReportingService.ReportConflictSummary(reducedConflicts);

        // remaining conflicts after reduction
        var sortConflictSummaryAfterReduction = new SortingConflictSummary
        {
            NonConflictingFiles = reducedConflicts.NonConflictingFiles
                .Concat(reducedConflicts.ReducedSortingConflicts
                    .Where(ConflictIsResolved)
                    .SelectMany(x => x.ChosenFiles))
                .ToArray(),
            Conflicts = reducedConflicts.ReducedSortingConflicts
                .Where(x => !ConflictIsResolved(x))
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

        var filesAtDestinationSet = filesAtDestination.ToHashSet();
        var filesToWrite = _conflictResolver.ResolveConflicts(sortConflictSummaryAfterReduction, filesAtDestinationSet);

        // TBD implement writing of results, pass overwrite true only if DestinationConflict is set to Overwrite

        // TBD        

        _logger.LogInformation("blub ({Sas})", filesToWrite);

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

    private bool ConflictIsResolved(ReducedSortingConflict reducedSortingConflict)
    {
        return reducedSortingConflict.IsResolved ||
               (_sortRunConfiguration.DestinationConflictMode == DestinationConflictMode.Overwrite &&
                reducedSortingConflict.IsResolvedInSource);
    }
}