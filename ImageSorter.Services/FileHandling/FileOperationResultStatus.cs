namespace ImageSorter.Services.FileHandling;

public enum FileOperationResultStatus
{
    /// <summary>
    /// Indicates that the file is already in the correect location, so no operation is needed
    /// </summary>
    /// <remarks>
    /// Can only happen while moving files
    /// </remarks>
    AlreadyInCorrectPlace,

    /// <summary>
    /// Moved / Copied file successfully and without conflicts.
    /// </summary>
    Success,
    
    /// <summary>
    /// Moved / Copied file successfully by overwriting the existing file with the destination file name.
    /// </summary>
    OverwriteSuccess,
    
    /// <summary>
    /// Skipped moving / copying the file since a file with the same name already exists at the destination directory.
    /// </summary>
    Skipped,
    
    /// <summary>
    /// An error occurred during the move / copy operation.
    /// </summary>
    Error
}