namespace ImageSorter.Sorting.Services.ConflictReducer;

public static class FileEqualityClassHelpers
{
    public static void MergeEqualityClasses(
        this List<FileEquivalenceClass> list,
        FileEquivalenceClass left,
        FileEquivalenceClass right)
    {
        foreach (var path in right.Paths)
        {
            left.Paths.Add(path);
        }

        list.Remove(right);
    }
}