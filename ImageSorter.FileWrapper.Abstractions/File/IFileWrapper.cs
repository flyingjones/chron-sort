using System.Diagnostics.CodeAnalysis;

namespace ImageSorter.FileWrapper.Abstractions.File;

/// <summary>
/// Wrapper for <see cref="File"/>
/// </summary>
public interface IFileWrapper
{
    /// <inheritdoc cref="File.Exists"/>
    bool Exists([NotNullWhen(true)] string? path);

    /// <inheritdoc cref="File.Move(string, string, bool)"/>
    void Move(string sourceFileName, string destFileName, bool overwrite);

    /// <summary>
    /// The length of a file in bytes.
    /// </summary>
    long FileSize(string path);
}