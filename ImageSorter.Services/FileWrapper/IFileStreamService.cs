namespace ImageSorter.Services.FileWrapper;

public interface IFileStreamService
{
    Task CopyToAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken);
}