namespace ImageSorter.Sorting.Services.EquivalenceClassFinder;

public interface IEquivalenceClassFinder
{
    /// <summary>
    /// Groups a collection of items into equivalence classes using the given <paramref name="equivalenceRelation"/>
    /// </summary>
    /// <remarks>
    /// This process could also be built using hashing so first we compute a hash for each file and then compare the hashes
    /// but this results in needing to read every file completely to compute the hash whereas this method can return
    /// as soon as the first difference is found
    /// <br/>
    /// If one has a lot of equal files, hash-based comparisons should be faster anyway
    /// </remarks>
    ICollection<EquivalenceClass<T>> GroupIntoEquivalenceClasses<T>(
        ICollection<T> items,
        Func<T, T, bool> equalityRelation,
        Func<T, T, bool> equivalenceRelation);
    
    Task<ICollection<EquivalenceClass<T>>> GroupIntoEquivalenceClasses<T>(
        ICollection<T> items,
        Func<T, T, bool> equalityRelation,
        Func<T, T, CancellationToken, Task<bool>> equivalenceRelation,
        CancellationToken cancellationToken);
}