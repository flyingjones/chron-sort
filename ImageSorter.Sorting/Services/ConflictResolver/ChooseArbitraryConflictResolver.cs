using ImageSorter.Sorting.Abstractions.Model;
using ImageSorter.Sorting.Abstractions.Services;

namespace ImageSorter.Sorting.Services.ConflictResolver;

/// <summary>
/// Resolves conflicts by choosing one file arbitrarily (it will choose existing files first though)
/// </summary>
public class ChooseArbitraryConflictResolver : IConflictResolver
{
    public ICollection<SortedFilePath> ResolveConflicts(SortingConflictSummary sortingConflictSummary, HashSet<string> filesAtDestination)
    {
        return sortingConflictSummary.NonConflictingFiles.Concat(
                sortingConflictSummary.Conflicts
                    .Where(x => x.ConflictingFiles.All(file => file.IsFromSource))
                    .Select(conflict => conflict.ConflictingFiles.First().SortedFilePath!))
            .ToArray();
    }
}