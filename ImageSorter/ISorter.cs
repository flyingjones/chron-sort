namespace ImageSorter;

public interface ISorter
{
    Task PerformSorting(bool moveFiles, CancellationToken cancellationToken);
}