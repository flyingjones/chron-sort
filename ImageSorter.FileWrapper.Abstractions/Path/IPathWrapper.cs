namespace ImageSorter.FileWrapper.Abstractions.Path;

/// <summary>
/// Wrapper for static class <see cref="System.IO.Path"/>
/// </summary>
public interface IPathWrapper
{
    /// <inheritdoc cref="System.IO.Path.GetFileName(string?)"/>
    string? GetFileName(string? path);

    /// <inheritdoc cref="System.IO.Path.Combine(string, string)"/>
    string Combine(string left, string right);
}