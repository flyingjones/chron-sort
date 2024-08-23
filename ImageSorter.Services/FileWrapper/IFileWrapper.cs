using System.Diagnostics.CodeAnalysis;

namespace ImageSorter.Services.FileWrapper;

public interface IFileWrapper
{
    /// <inheritdoc cref="File.Exists"/>
    bool Exists([NotNullWhen(true)] string? path);

    /// <inheritdoc cref="File.Move(string, string, bool)"/>
    void Move(string sourceFileName, string destFileName, bool overwrite);
}