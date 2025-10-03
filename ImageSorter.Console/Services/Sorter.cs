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
        // find all relevant files in the source directory and filter them by file extension if filtering is used
        _logger.LogInformation("Scanning Source Directory");
        var filesToProcess = _fileLoader.GetFilePaths(_sortRunConfiguration.SourcePath, out var allFiles);
        _summaryReportingService.ReportScanSummary(filesToProcess, allFiles);

        // assign a date to each file using the given configuration
        _logger.LogInformation("Parsing Dates");
        var parsedFiles = (await _dateParsingHandler.ScanFiles(filesToProcess, cancellationToken))
            .Select(x => new FilePathWithParsedDate
            {
                FilePath = x.FilePath,
                DateTime = x.DateTaken
            })
            .ToArray();

        // group the files according to the given output path and the grouping pattern, e.g. yyyy/MM
        _logger.LogInformation("Sorting Files");
        var rawSorting = _newSorter.SortFiles(parsedFiles);

        // also load the file paths at the destination directory for better conflict handling
        _logger.LogInformation("Scanning Destination Directory");
        string[] filesAtDestination = _fileLoader.GetFilePaths(_sortRunConfiguration.DestinationPath, out var _);

        // now find conflicts where multiple files with the same name appear in the same directory after the grouping step
        // this can happen if files are already present there and / or when files from different paths in the source
        // have the same name and same date
        _logger.LogInformation("Searching for Conflicts");
        var conflicts = _conflictFinder.FindConflicts(rawSorting, filesAtDestination);

        // try to reduce the conflicts as much as possible by e.g. skipping files which are equivalent
        // the exact methods can be configured and differ in speed and accuracy
        _logger.LogInformation("Performing Conflict Reduction");
        var reducedConflicts = _conflictReducer.ReduceConflicts(conflicts);
        _summaryReportingService.ReportConflictSummary(reducedConflicts);

        // remaining conflicts after reduction
        var sortConflictSummaryAfterReduction = RemoveObsoleteConflicts(reducedConflicts);

        // resolve conflicts by e.g. renaming files, choosing arbitrarily or throwing an exception
        var filesToWrite = _conflictResolver.ResolveConflicts(
            sortConflictSummaryAfterReduction,
            filesAtDestination,
            out var discardedFiles);
        _summaryReportingService.ReportConflictResolution(filesToWrite, discardedFiles, sortConflictSummaryAfterReduction);

        var writeDtos = filesToWrite
            .Select(x => new FileWriteDto
            {
                // this ensures that the casing of the writes are consistent with the input
                // otherwise we may inadvertently change the casing of the file name after the operation
                DestinationPath = _sortRunConfiguration.OriginalDestinationPath +
                                  x.DestinationFilePath[_sortRunConfiguration.OriginalDestinationPath.Length..],
                SourcePath = x.SourceFilePath,
                DateTime = x.DateTime
            }).ToArray();

        var writeResults = await _resultWriter.Write(writeDtos, default);

        // TODO implement error handling

        // TODO implement write result reporting (maybe add option to exactly write which file got where and
        // what happened to it) summarize total ops including renames

        // TODO unit tests

        // todo docs
        _summaryReportingService.ReportWriteSummary(writeResults);
        _logger.LogInformation("Finished");
    }

    private SortingConflictSummary RemoveObsoleteConflicts(
        ReducedSortingConflictSummary reducedConflicts)
    {
        return new SortingConflictSummary
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
    }

    private bool ConflictIsResolved(ReducedSortingConflict reducedSortingConflict)
    {
        return reducedSortingConflict.IsResolved ||
               (_sortRunConfiguration.DestinationConflictMode == DestinationConflictMode.Overwrite &&
                reducedSortingConflict.IsResolvedInSource);
    }
}