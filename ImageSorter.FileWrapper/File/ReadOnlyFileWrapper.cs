
using ImageSorter.FileWrapper.Abstractions.File;

namespace ImageSorter.FileHandling.File;

/// <summary>
/// Implementation of <see cref="IFileWrapper"/> which only implements the reading methods and does nothing in writing
/// methods.
/// </summary>
public class ReadOnlyFileWrapper : IFileWrapper
{
    /// <inheritdoc cref="IFileWrapper.Exists"/>
    public bool Exists(string? path)
    {
        return System.IO.File.Exists(path);
    }

    /// <summary>
    /// Doesn't do anything
    /// </summary>
    public void Move(string sourceFileName, string destFileName, bool overwrite)
    {
        // no op
    }
}