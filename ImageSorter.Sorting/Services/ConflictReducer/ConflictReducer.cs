using ImageParser.Utils.Math;
using ImageParser.Utils.ProgressLogger;
using ImageSorter.Sorting.Abstractions.Model;
using ImageSorter.Sorting.Abstractions.Services;

namespace ImageSorter.Sorting.Services.ConflictReducer;

public class ConflictReducer : IConflictReducer
{
    private readonly IFileEquivalenceMetricImplementation _fileEquivalenceMetricImplementation;
    private readonly IProgressLogger<ConflictReducer> _progressLogger;
    private readonly IDestinationConflictQuickResolver _destinationConflictQuickResolver;

    public ConflictReducer(
        IFileEquivalenceMetricImplementation fileEquivalenceMetricImplementation,
        IProgressLogger<ConflictReducer> progressLogger,
        IDestinationConflictQuickResolver destinationConflictQuickResolver)
    {
        _fileEquivalenceMetricImplementation = fileEquivalenceMetricImplementation;
        _progressLogger = progressLogger;
        _destinationConflictQuickResolver = destinationConflictQuickResolver;
    }

    public ReducedSortingConflict ReduceConflicts(SortingConflict sortingConflict)
    {
        // this algorithm finds the equivalence classes for all input files using the injected
        // equivalence metric. (after performing a quick resolve)
        
        if (_destinationConflictQuickResolver.TryQuickResolve(sortingConflict, out var reducedConflict))
        {
            return reducedConflict;
        }
        
        var equalityClasses = sortingConflict
            .ConflictingFiles
            .Select(x => new FileEquivalenceClass(x))
            .ToList();

        var conflictingFilesArray = sortingConflict.ConflictingFiles.ToArray();
        
        // think of the given files as a matrix where each entry represents a comparison
        // we only need to check one triangle of the matrix without the diagonal:
        // we know that if ( A == B ) then ( B == A) and also that ( A == A ) is always true
        
        // we could optimize this further if we track the different files for each equivalence class explicitly and
        // merge those as well. Maybe get someone to get the paper
        // https://link.springer.com/chapter/10.1007/978-3-319-21840-3_36
        // doi:10.1007/978-3-319-21840-3_36
        for (int i = 0; i < conflictingFilesArray.Length; i++)
        {
            var leftEqualityClass = equalityClasses
                .First(x => x.RepresentsPath(conflictingFilesArray[i]));
            
            for (int j = i + 1; j < conflictingFilesArray.Length; j++)
            {
                var rightEqualityClass = equalityClasses
                    .First(x => x.RepresentsPath(conflictingFilesArray[j]));

                if (ReferenceEquals(leftEqualityClass, rightEqualityClass))
                {
                    // happens when we get equality classes with size more than 2
                    // we already found the equivalence, so we just continue here
                    continue;
                }
                
                var classesAreEqual = _fileEquivalenceMetricImplementation.FilesAreEquivalent(
                    leftEqualityClass.RepresentativePath,
                    rightEqualityClass.RepresentativePath);

                if (classesAreEqual)
                {
                    equalityClasses.MergeEqualityClasses(leftEqualityClass, rightEqualityClass);
                }
            }
        }

        var chosenFiles = equalityClasses
            .Where(x => !x.IsRepresentedAtDestination)
            // the ordering makes this step deterministic since the same will be chosen in every run for the same class
            .Select(equalityClass => equalityClass.Paths.OrderBy(x => x.SourceFilePath).First())
            // since only classes are taken which contain only non destination elements, we can always select to the 
            // source specific type
            .Select(equalityClass => equalityClass.SortedFilePath!)
            .ToArray();

        var discardedFiles = equalityClasses
            .SelectMany(x => x.Paths)
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

    public ReducedSortingConflictSummary ReduceConflicts(SortingConflictSummary sortingConflictSummary)
    {
        // we need at max BinomialCoefficient(n, 2) comparisons since we need to check for each unique pair of files if
        // they are equal if all files are different.
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
            reducedConflicts[idx] = ReduceConflicts(conflict);
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
}