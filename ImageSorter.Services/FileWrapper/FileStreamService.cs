namespace ImageSorter.Services.FileWrapper;

/// <inheritdoc cref="IFileStreamService"/>
public class FileStreamService : IFileStreamService
{
    /// <inheritdoc cref="IFileStreamService.CopyToAsync"/>
    public async Task CopyToAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken)
    {
        await using var sourceFileStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
        await using var destFileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);

        await sourceFileStream.CopyToAsync(destFileStream, cancellationToken);
    }
}