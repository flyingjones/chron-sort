namespace ImageSorter.FileHandling.File;

/// <inheritdoc cref="IFileWrapper"/>
public class FileWrapper : IFileWrapper
{
    /// <inheritdoc cref="IFileWrapper.Exists"/>
    public bool Exists(string? path)
    {
        return System.IO.File.Exists(path);
    }

    /// <inheritdoc cref="IFileWrapper.Move"/>
    public void Move(string sourceFileName, string destFileName, bool overwrite)
    {
        System.IO.File.Move(sourceFileName, destFileName, overwrite);
    }

    /// <inheritdoc cref="IFileWrapper.FileLength"/>
    public long FileLength(string path)
    {
        return new FileInfo(path).Length;
    }
}