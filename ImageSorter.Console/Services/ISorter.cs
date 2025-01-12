namespace ImageSorter.Services;

public interface ISorter
{
    Task PerformSorting(bool moveFiles, CancellationToken cancellationToken);
}