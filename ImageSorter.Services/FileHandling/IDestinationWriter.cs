namespace ImageSorter.Services.FileHandling;

public interface IDestinationWriter
{
    /// <summary>
    /// Copies a file to the destination year/month directory
    /// </summary>
    Task CopyFile(string sourcePath, DateTime dateTime, CancellationToken cancellationToken);
}