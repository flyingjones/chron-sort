using System.Diagnostics.CodeAnalysis;

namespace ImageSorter.FileWrapper.Abstractions.Path;

/// <summary>
/// Wrapper for static class <see cref="System.IO.Path"/>
/// </summary>
public interface IPathWrapper
{
    /// <inheritdoc cref="System.IO.Path.GetFileName(string?)"/>
    [return: NotNullIfNotNull("path")]
    string? GetFileName( string? path);

    /// <inheritdoc cref="System.IO.Path.GetFileNameWithoutExtension(string?)"/>
    [return: NotNullIfNotNull("path")]
    public string? GetFileNameWithoutExtension(string? path);

    /// <inheritdoc cref="System.IO.Path.GetExtension(string?)"/>
    [return: NotNullIfNotNull("path")]
    public string? GetExtension(string? path);

    /// <inheritdoc cref="System.IO.Path.Combine(string, string)"/>
    string Combine(string left, string right);
}