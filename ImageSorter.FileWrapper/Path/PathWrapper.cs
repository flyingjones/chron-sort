using System.Diagnostics.CodeAnalysis;
using ImageSorter.FileWrapper.Abstractions.Path;

namespace ImageSorter.FileHandling.Path;

/// <inheritdoc/>
public class PathWrapper : IPathWrapper
{
    /// <inheritdoc/>
    [return: NotNullIfNotNull("path")]
    public string? GetFileName(string? path)
    {
        return System.IO.Path.GetFileName(path);
    }
    
    /// <inheritdoc/>
    [return: NotNullIfNotNull("path")]
    public string? GetFileNameWithoutExtension(string? path)
    {
        return System.IO.Path.GetFileNameWithoutExtension(path);
    }
    
    /// <inheritdoc/>
    [return: NotNullIfNotNull("path")]
    public string? GetExtension(string? path)
    {
        return System.IO.Path.GetExtension(path);
    }

    /// <inheritdoc/>
    public string Combine(string left, string right)
    {
        return System.IO.Path.Combine(left, right);
    }
}