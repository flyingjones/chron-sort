using ImageSorter.DateParsing.Abstractions.Model.MetaData;

namespace ImageSorter.Services.FileHandling;

/// <summary>
/// Service for writing the sorting to disk
/// </summary>
/// <remarks>
/// Is configured by <see cref="DestinationWriterOptions"/>
/// </remarks>
public interface IDestinationWriter
{
    /// <summary>
    /// Copy the <paramref name="parsedFileResults"/> to the destination in the directory structure year/month
    /// </summary>
    Task<ICollection<FileOperationResult>> CopyFiles(ICollection<ParsedFileResult> parsedFileResults, CancellationToken cancellationToken);

    /// <summary>
    /// Move the <paramref name="parsedFileResults"/> in the source directory to the directory structure year/month
    /// </summary>
    ICollection<FileOperationResult> MoveFiles(ICollection<ParsedFileResult> parsedFileResults, CancellationToken cancellationToken);
}