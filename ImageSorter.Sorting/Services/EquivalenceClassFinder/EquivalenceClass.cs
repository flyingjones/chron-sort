namespace ImageSorter.Sorting.Services.EquivalenceClassFinder;

/// <summary>
/// Represents some items which are equivalent according to some metric (and may also be equal)
/// </summary>
/// <typeparam name="T">Type of the item</typeparam>
public class EquivalenceClass<T>
{
    public EquivalenceClass(T firstItem)
    {
        Items = new List<T> { firstItem };
    }
    
    /// <summary>
    /// The items which are equivalent
    /// </summary>
    public ICollection<T> Items { get; set; }

    /// <summary>
    /// The item which represents the class (just the first item).
    /// <br/>
    /// Used to perform the comparison operations
    /// </summary>
    public T RepresentativeElement => Items.First();

    /// <summary>
    /// Checks if the given <paramref name="item"/> is already present (by using <see cref="ICollection{T}.Contains"/>)
    /// in the equivalence class
    /// </summary>
    public bool Represents(T item)
    {
        return Items.Contains(item);
    }
}