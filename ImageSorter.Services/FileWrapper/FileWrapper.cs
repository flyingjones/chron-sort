namespace ImageSorter.Services.FileWrapper;

/// <inheritdoc cref="IFileWrapper"/>
public class FileWrapper : IFileWrapper
{
    /// <inheritdoc cref="IFileWrapper.Exists"/>
    public bool Exists(string? path)
    {
        return File.Exists(path);
    }

    /// <inheritdoc cref="IFileWrapper.Move"/>
    public void Move(string sourceFileName, string destFileName, bool overwrite)
    {
        File.Move(sourceFileName, destFileName, overwrite);
    }
}