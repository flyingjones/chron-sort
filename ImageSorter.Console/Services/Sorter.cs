using System.Reflection;
using ImageSorter.DateParsing.Abstractions.Services;
using ImageSorter.DependencyInjection;
using ImageSorter.ResultWriting.Abstractions;
using ImageSorter.ResultWriting.Abstractions.Model;
using ImageSorter.Services.FileHandling;
using ImageSorter.Sorting.Abstractions.Model;
using ImageSorter.Sorting.Abstractions.Services;
using ImageSorter.Sorting.Model;
using ImageSorter.Sorting.Services;
using ImageSorter.Sorting.Services.FilePath;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Services;

public partial class Sorter : ISorter
{
    private readonly SortRunConfiguration _sortRunConfiguration;
    private readonly ILogger<Sorter> _logger;
    private readonly IFileLoader _fileLoader;
    private readonly IDateParsingHandler _dateParsingHandler;
    private readonly ISummaryReportingService _summaryReportingService;
    private readonly Sorting.Abstractions.Services.ISorter _newSorter;
    private readonly IConflictFinder _conflictFinder;
    private readonly IConflictReducer _conflictReducer;
    private readonly IConflictResolver _conflictResolver;
    private readonly IResultWriter _resultWriter;
    private readonly IFilePathWrapperFactory _filePathWrapperFactory;

    public Sorter(
        ILogger<Sorter> logger,
        IFileLoader fileLoader,
        IDateParsingHandler dateParsingHandler,
        ISummaryReportingService summaryReportingService,
        Sorting.Abstractions.Services.ISorter newSorter,
        IConflictFinder conflictFinder,
        IConflictReducer conflictReducer,
        SortRunConfiguration sortRunConfiguration,
        IConflictResolver conflictResolver,
        IResultWriter resultWriter,
        IFilePathWrapperFactory filePathWrapperFactory)
    {
        _logger = logger;
        _fileLoader = fileLoader;
        _dateParsingHandler = dateParsingHandler;
        _summaryReportingService = summaryReportingService;
        _newSorter = newSorter;
        _conflictFinder = conflictFinder;
        _conflictReducer = conflictReducer;
        _sortRunConfiguration = sortRunConfiguration;
        _conflictResolver = conflictResolver;
        _resultWriter = resultWriter;
        _filePathWrapperFactory = filePathWrapperFactory;
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
                        .Select(chosenFile => _filePathWrapperFactory.Create(chosenFile))
                        .Concat(x.ConflictingFiles.Where(y => !y.IsFromSource))
                        .ToArray()
                })
                .ToArray()
        };

        var filesToWrite = _conflictResolver.ResolveConflicts(sortConflictSummaryAfterReduction, filesAtDestination);

        var writeDtos = filesToWrite.Select(x => new FileWriteDto
        {
            // this ensures that the casing of the writes are consistent with the input
            DestinationPath = _sortRunConfiguration.OriginalDestinationPath + x.DestinationFilePath[_sortRunConfiguration.OriginalDestinationPath.Length..],
            SourcePath = x.SourceFilePath,
            DateTime = x.DateTime
        }).ToArray();
        
        var writeResults = await _resultWriter.Write(writeDtos, default);


        // TODO implement smart case sensitivity handling ->
        // check before doing anything else by creating a file at dest
        // then configure everything to return toLower for paths or not, depending on file system case sensitivity
        
        // TODO implement error handling
        
        // TODO implement write result reporting (maybe add option to exactly write which file got where and
        // what happened to it) summarize total ops inkluding renames
        
        // TODO unit tests
        
        // todo docs
        _summaryReportingService.ReportWriteSummary(writeResults);
        _logger.LogInformation("Finished");
    }

    private bool ConflictIsResolved(ReducedSortingConflict reducedSortingConflict)
    {
        return reducedSortingConflict.IsResolved ||
               (_sortRunConfiguration.DestinationConflictMode == DestinationConflictMode.Overwrite &&
                reducedSortingConflict.IsResolvedInSource);
    }
}