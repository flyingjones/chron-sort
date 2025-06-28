using ImageSorter.FileWrapper.Abstractions.Path;

namespace ImageSorter.FileHandling.Path;

/// <inheritdoc/>
public class PathWrapper : IPathWrapper
{
    /// <inheritdoc/>
    public string? GetFileName(string? path)
    {
        return System.IO.Path.GetFileName(path);
    }

    /// <inheritdoc/>
    public string Combine(string left, string right)
    {
        return System.IO.Path.Combine(left, right);
    }
}