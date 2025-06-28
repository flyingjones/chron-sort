using System.Text;
using ImageSorter.FileWrapper.Abstractions.Directory;
using ImageSorter.FileWrapper.Abstractions.Path;
using ImageSorter.Sorting.Abstractions.Model;
using ImageSorter.Sorting.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Sorting.Services.ConflictResolver;

/// <summary>
/// resolves conflicts by renaming files with prepending their parent directory name (recursively if still conflicting)
/// </summary>
/// <example>
/// File <c>italy/house/img_01.jpg</c> becomes <c>house_img01.jpg</c> and if it is still conflicting
/// <c>italy_house_img_01.jpg</c>
/// </example>
public class SemanticRenameConflictResolver : AbstractRenameBasedConflictResolver
{
    private readonly IPathWrapper _pathWrapper;
    private readonly IDirectoryWrapper _directoryWrapper;
    private readonly ILogger<SemanticRenameConflictResolver> _logger;

    public SemanticRenameConflictResolver(
        IPathWrapper pathWrapper,
        IDirectoryWrapper directoryWrapper,
        ILogger<SemanticRenameConflictResolver> logger) : base(pathWrapper, directoryWrapper, logger)
    {
        _pathWrapper = pathWrapper;
        _directoryWrapper = directoryWrapper;
        _logger = logger;
    }

    protected override ICollection<KeyValuePair<SortedFilePath, string>> GetProposedRenameNames(
        IEnumerable<SortedFilePath> filePaths, int depth)
    {
        return filePaths.Select(x => new KeyValuePair<SortedFilePath, string>(
                x,
                BuildAltFileName(GetDirectories(x.SourceFilePath), _pathWrapper.GetFileName(x.SourceFilePath)!, depth)))
            .ToArray();
    }

    private string[] GetDirectories(string fileName)
    {
        var parentDir = _directoryWrapper.GetParentDirectory(fileName);
        if (parentDir == null) return [];

        return parentDir.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries);
    }

    private string BuildAltFileName(string[] sourceDirectoryPath, string currentFileName, int depth)
    {
        if (depth > sourceDirectoryPath.Length)
        {
            var sourcePath = string.Join(Path.DirectorySeparatorChar, sourceDirectoryPath);
            _logger.LogError("Could not rename file {FileName} because its source path {Path} is not distinct enough!",
                currentFileName, sourcePath);
            throw new ArgumentException(
                $"Could not rename file {currentFileName} because its source path {sourcePath} is not distinct enough!");
        }

        var builder = new StringBuilder();
        for (int i = sourceDirectoryPath.Length - depth; i < sourceDirectoryPath.Length; i++)
        {
            builder.Append($"{sourceDirectoryPath[i]}_");
        }

        builder.Append(currentFileName);
        return builder.ToString();
    }
}