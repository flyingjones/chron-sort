namespace ImageSorter.Services.FileWrapper;

public class FileStreamService : IFileStreamService
{
    public async Task CopyToAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken)
    {
        await using var sourceFileStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
        await using var destFileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);

        await sourceFileStream.CopyToAsync(destFileStream, cancellationToken);
    }
}