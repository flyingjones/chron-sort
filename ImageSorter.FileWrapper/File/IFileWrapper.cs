using System.Diagnostics.CodeAnalysis;

namespace ImageSorter.FileHandling.File;

/// <summary>
/// Wrapper for <see cref="File"/>
/// </summary>
public interface IFileWrapper
{
    /// <inheritdoc cref="File.Exists"/>
    bool Exists([NotNullWhen(true)] string? path);

    /// <inheritdoc cref="File.Move(string, string, bool)"/>
    void Move(string sourceFileName, string destFileName, bool overwrite);

    /// <inheritdoc cref="FileInfo.Length"/>
    long FileLength(string path);
}