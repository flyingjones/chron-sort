using ImageParser.Utils.Math;
using ImageParser.Utils.ProgressLogger;
using ImageSorter.Sorting.Abstractions.Model;
using ImageSorter.Sorting.Abstractions.Services;
using ImageSorter.Sorting.Services.EquivalenceClassFinder;

namespace ImageSorter.Sorting.Services.ConflictReducer;

public class ConflictReducer : IConflictReducer
{
    private readonly IFileEquivalenceMetricImplementation _fileEquivalenceMetricImplementation;
    private readonly IProgressLogger<ConflictReducer> _progressLogger;
    private readonly IDestinationConflictQuickResolver _destinationConflictQuickResolver;
    private readonly IEquivalenceClassFinder _equivalenceClassFinder;

    public ConflictReducer(
        IFileEquivalenceMetricImplementation fileEquivalenceMetricImplementation,
        IProgressLogger<ConflictReducer> progressLogger,
        IDestinationConflictQuickResolver destinationConflictQuickResolver,
        IEquivalenceClassFinder equivalenceClassFinder)
    {
        _fileEquivalenceMetricImplementation = fileEquivalenceMetricImplementation;
        _progressLogger = progressLogger;
        _destinationConflictQuickResolver = destinationConflictQuickResolver;
        _equivalenceClassFinder = equivalenceClassFinder;
    }

    public async Task<ReducedSortingConflict> ReduceConflicts(SortingConflict sortingConflict, CancellationToken cancellationToken)
    {
        if (_destinationConflictQuickResolver.TryQuickResolve(sortingConflict, out var reducedConflict))
        {
            return reducedConflict;
        }

        var equalityClasses = await _equivalenceClassFinder.GroupIntoEquivalenceClasses(
            sortingConflict.ConflictingFiles,
            // files are equal if they have the same source file path
            (a, b) => a.NormalizedSourceFilePath == b.NormalizedSourceFilePath,
            // files are equivalent if the given equivalence relation says so
            (a, b, innerToken) => _fileEquivalenceMetricImplementation.FilesAreEquivalent(
                a.SourceFilePath,
                b.SourceFilePath,
                innerToken),
            cancellationToken
        );

        var chosenFiles = equalityClasses
            .Where(x => !IsRepresentedAtDestination(x))
            // the ordering makes this step deterministic since the same will be chosen in every run for the same class
            .Select(equalityClass => equalityClass.Items.OrderBy(x => x.SourceFilePath).First())
            // since only classes are taken which contain only non destination elements, we can always select to the 
            // source specific type
            .Select(equalityClass => equalityClass.SortedFilePath!)
            .ToArray();

        var discardedFiles = equalityClasses
            .SelectMany(x => x.Items)
            .Where(x => x.IsFromSource)
            .Select(x => x.SortedFilePath!)
            .Where(x => !chosenFiles.Contains(x))
            .ToArray();

        return new ReducedSortingConflict
        {
            ChosenFiles = chosenFiles,
            ConflictingFiles = sortingConflict.ConflictingFiles,
            DiscardedFiles = discardedFiles
        };
    }

    public async Task<ReducedSortingConflictSummary> ReduceConflicts(SortingConflictSummary sortingConflictSummary, CancellationToken cancellationToken)
    {
        // we need at max BinomialCoefficient(n, 2) comparisons since we need to check for each unique pair of files if
        // they are equal if all files are different.
        // this is just the worst case, we may not need to perform all comparisons.
        var totalCompareAmount = sortingConflictSummary
            .Conflicts.Select(conflict =>
                BinomialCoefficient.CalculateBinomialCoefficient(conflict.ConflictingFiles.Count, 2))
            .Sum();

        _progressLogger.LogStart("Reducing {Count} conflicts (this may take a while)", totalCompareAmount);
        var reducedConflicts = new ReducedSortingConflict[sortingConflictSummary.Conflicts.Count];

        var completedCompares = 0L;
        var idx = 0;
        foreach (var conflict in sortingConflictSummary.Conflicts)
        {
            completedCompares +=
                BinomialCoefficient.CalculateBinomialCoefficient(conflict.ConflictingFiles.Count, 2);
            reducedConflicts[idx] = await ReduceConflicts(conflict, cancellationToken);
            idx++;
            
            _progressLogger.LogProgress((double) completedCompares / totalCompareAmount);
        }
        
        _progressLogger.LogProgressFinished();

        return new ReducedSortingConflictSummary
        {
            NonConflictingFiles = sortingConflictSummary.NonConflictingFiles,
            ReducedSortingConflicts = reducedConflicts
        };
    }

    private static bool IsRepresentedAtDestination(EquivalenceClass<FilePathWrapper> equivalenceClass)
    {
        return equivalenceClass.Items.Any(x => !x.IsFromSource);
    }
}