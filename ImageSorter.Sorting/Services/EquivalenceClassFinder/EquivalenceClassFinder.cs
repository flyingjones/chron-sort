namespace ImageSorter.Sorting.Services.EquivalenceClassFinder;

public class EquivalenceClassFinder : IEquivalenceClassFinder
{
    /// <inheritdoc/>
    public ICollection<EquivalenceClass<T>> GroupIntoEquivalenceClasses<T>(
        ICollection<T> items,
        Func<T, T, bool> equalityRelation,
        Func<T, T, bool> equivalenceRelation)
    {
        var asyncEquivalenceRelation =
            new Func<T, T, CancellationToken, Task<bool>>((left, right, _) =>
                Task.FromResult(equalityRelation(left, right)));
        return GroupIntoEquivalenceClasses(items, equalityRelation, asyncEquivalenceRelation, CancellationToken.None).Result;
    }
    
    /// <inheritdoc/>
    public async Task<ICollection<EquivalenceClass<T>>> GroupIntoEquivalenceClasses<T>(
        ICollection<T> items,
        Func<T, T, bool> equalityRelation,
        Func<T, T, CancellationToken, Task<bool>> equivalenceRelation,
        CancellationToken cancellationToken)
    {
        var itemArray = items.ToArray();

        // first assemble the initial equivalence classes (one for each item)
        // wrap those into key value pairs so we can save a numerical index
        var equivalenceClasses = items
            .Select(x => new EquivalenceClass<T>(x))
            .ToList();

        var skippedIndexes = new List<int>();

        for (int i = 0; i < itemArray.Length; i++)
        {
            if (skippedIndexes.Contains(i))
            {
                continue;
            }

            var leftEquivalenceClass = equivalenceClasses.First(x => x.Represents(itemArray[i]));

            for (int j = i + 1; j < itemArray.Length; j++)
            {
                var rightEquivalenceClass = equivalenceClasses.First(x => x.Represents(itemArray[j]));

                if (ReferenceEquals(leftEquivalenceClass, rightEquivalenceClass))
                {
                    // we may actually have true equality here so we need to add the item to the first class, and remove all others
                    var classesToMerge = equivalenceClasses
                        .Where(x => equalityRelation(x.RepresentativeElement,
                            leftEquivalenceClass.RepresentativeElement))
                        .Skip(1)
                        .ToArray();
                    foreach (var classToMerge in classesToMerge)
                    {
                        MergeEqualityClasses(equivalenceClasses, leftEquivalenceClass, classToMerge);
                        // we now know that i ~= j so we can also assume that the given equivalency checks for j are obsolete
                        // since we already did those for i
                        skippedIndexes.Add(j);
                    }

                    continue;
                }

                var classesAreEqual = await equivalenceRelation(
                    leftEquivalenceClass.RepresentativeElement,
                    rightEquivalenceClass.RepresentativeElement,
                    cancellationToken);

                if (classesAreEqual)
                {
                    MergeEqualityClasses(equivalenceClasses, leftEquivalenceClass, rightEquivalenceClass);

                    // we now know that i ~= j so we can also assume that the given equivalency checks for j are obsolete
                    // since we already did those for i
                    skippedIndexes.Add(j);
                }
            }
        }

        return equivalenceClasses;
    }

    private static void MergeEqualityClasses<T>(
        List<EquivalenceClass<T>> list,
        EquivalenceClass<T> left,
        EquivalenceClass<T> right)
    {
        foreach (var path in right.Items)
        {
            left.Items.Add(path);
        }

        list.Remove(right);
    }
}