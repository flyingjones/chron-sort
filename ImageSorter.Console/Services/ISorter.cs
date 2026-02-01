namespace ImageSorter.Services;

public interface ISorter
{
    Task PerformSorting(CancellationToken cancellationToken);
}