using System.Diagnostics.CodeAnalysis;
using System.IO.Enumeration;

namespace ImageSorter.Tests.InMemory;

public class InMemoryFileSystem
{
    private readonly InMemoryDirectory _rootDirectory;

    public InMemoryFileSystem()
    {
        _rootDirectory = new InMemoryDirectory(null, RootDirName);
    }
    
    public static string RootDirName => Path.DirectorySeparatorChar == '\\' ? "M:" : "";

    private static string NormalizePath(string path)
    {
        var normalizedPath = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        if (normalizedPath.Contains(':'))
        {
            normalizedPath = normalizedPath.Split(':', 2)[1];
        }
        
        return normalizedPath.StartsWith(Path.DirectorySeparatorChar) ? normalizedPath[1..] : normalizedPath;
    }

    private InMemoryDirectory? GetDirectory(string path)
    {
        if (path == null)
        {
            throw new ArgumentNullException();
        }

        var normalizedPath = NormalizePath(path);
        var directoryArray = normalizedPath.Split(Path.DirectorySeparatorChar);

        var currentDirectory = _rootDirectory;
        foreach (var level in directoryArray)
        {
            if (!currentDirectory.Directories.TryGetValue(level, out var directory))
            {
                return null;
            }

            currentDirectory = directory;
        }

        return currentDirectory;
    }

    /// <summary>
    /// Creates all directories and subdirectories in the specified path unless they already exist.
    /// </summary>
    public void CreateDirectory(string path)
    {
        if (path == null)
        {
            throw new ArgumentNullException();
        }

        var normalizedPath = NormalizePath(path);
        var directoryArray = normalizedPath.Split(Path.DirectorySeparatorChar);

        var currentDirectory = _rootDirectory;
        foreach (var level in directoryArray)
        {
            if (!currentDirectory.Directories.ContainsKey(level))
            {
                currentDirectory.AddDirectory(level);
            }

            currentDirectory = currentDirectory.Directories[level];
        }
    }

    /// <summary>
    /// Deletes an empty directory
    /// </summary>
    public void Delete(string path)
    {
        var directory = GetDirectory(path);
        if (directory == null)
        {
            throw new DirectoryNotFoundException($"directory {path} not found");
        }

        if (directory.Parent == null)
        {
            throw new IOException("Can't delete root directory");
        }

        if (!directory.IsEmpty)
        {
            throw new IOException($"directory {path} is not empty");
        }

        directory.Parent.Directories.Remove(directory.Name);
    }

    public IEnumerable<string> EnumerateDirectories(string path)
    {
        var directory = GetDirectory(path);
        if (directory == null)
        {
            throw new DirectoryNotFoundException($"directory {path} not found");
        }

        return directory.Directories.Keys;
    }

    public IEnumerable<string> EnumerateFileSystemEntries(string path)
    {
        var directory = GetDirectory(path);
        if (directory == null)
        {
            throw new DirectoryNotFoundException($"directory {path} not found");
        }

        return directory.Directories.Keys.Concat(directory.Files.Keys);
    }

    public string? GetParentDirectory(string path)
    {
        var directory = GetDirectory(path);
        if (directory == null)
        {
            throw new DirectoryNotFoundException($"directory {path} not found");
        }
        return directory.Parent?.Path;
    }

    public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        var directory = GetDirectory(path);
        if (directory == null)
        {
            throw new DirectoryNotFoundException($"directory {path} not found");
        }
        return GetFiles(directory, searchPattern, searchOption);
    }
    
    private string[] GetFiles(InMemoryDirectory directory, string searchPattern, SearchOption searchOption)
    {
        var filesInCurrentDir = directory.Files.Values
            .Where(file => FileSystemName.MatchesSimpleExpression(searchPattern, file.Name))
            .Select(file => file.Path)
            .ToList();

        if (searchOption == SearchOption.TopDirectoryOnly)
        {
            return filesInCurrentDir.ToArray();
        }

        foreach (var subDirectory in directory.Directories.Values)
        {
            filesInCurrentDir.AddRange(GetFiles(subDirectory, searchPattern, searchOption));
        }

        return filesInCurrentDir.ToArray();
    }

    private InMemoryDirectory? GetParentDirOfFilePath(string path)
    {
        // get the path of the parent directory of the file
        var pathArray = NormalizePath(path).Split(Path.DirectorySeparatorChar);

        InMemoryDirectory directory;
        if (pathArray.Length == 1)
        {
            return _rootDirectory;
        }
        else
        {
            return GetDirectory(string.Join('/', pathArray[..^1]));
        }
    }

    private static string GetFileName(string path)
    {
        return NormalizePath(path).Split(Path.DirectorySeparatorChar)[^1];
    }

    public InMemoryFile? GetFile(string path)
    {
        var directory = GetParentDirOfFilePath(path);
        var fileName = GetFileName(path);

        if (directory != null && directory.Files.TryGetValue(fileName, out var file))
        {
            return file;
        }

        return null;
    }

    public bool FileExists([NotNullWhen(true)] string? path)
    {
        if (path == null)
        {
            return false;
        }
        
        return GetFile(path) != null;
    }

    public long FileSize(string path)
    {
        return GetFile(path)!.Content.Length;
    }

    public void MoveFile(string sourceFileName, string destFileName, bool overwrite)
    {
        if (sourceFileName == null || destFileName == null)
        {
            throw new ArgumentNullException();
        }
        
        var sourceFile = GetFile(sourceFileName);
        if (sourceFile == null)
        {
            throw new FileNotFoundException($"File {sourceFileName} not found");
        }

        var destinationDir = GetParentDirOfFilePath(destFileName);
        if (destinationDir == null)
        {
            throw new DirectoryNotFoundException($"directory {destFileName} not found");
        }

        var destinationFileNameWithoutPath = GetFileName(destFileName);
        var fileExistsAtDestination = destinationDir.Files.ContainsKey(destinationFileNameWithoutPath);

        if (fileExistsAtDestination && !overwrite)
        {
            throw new IOException($"File already exists at {destFileName}");
        }

        var sourceParentDir = sourceFile.Parent;
        sourceParentDir.Files.Remove(sourceFile.Name);
        
        sourceFile.Name = destinationFileNameWithoutPath;
        destinationDir.Files[destinationFileNameWithoutPath] = sourceFile;
    }

    public void CopyFile(string sourceFileName, string destinationFileName)
    {
        if (sourceFileName == null || destinationFileName == null)
        {
            throw new ArgumentNullException();
        }
        
        var sourceFile = GetFile(sourceFileName);
        if (sourceFile == null)
        {
            throw new FileNotFoundException($"File {sourceFileName} not found");
        }

        var destinationDir = GetParentDirOfFilePath(destinationFileName);
        if (destinationDir == null)
        {
            throw new DirectoryNotFoundException($"Directory of file {destinationFileName} not found");
        }

        var destFileName = GetFileName(destinationFileName);
        if (destinationDir.Files.TryGetValue(destFileName, out var destFile))
        {
            destFile.Content = sourceFile.Content;
            destFile.DateTime = sourceFile.DateTime;
        }
        else
        {
            var copiedFile = new InMemoryFile
            {
                Content = sourceFile.Content,
                DateTime = sourceFile.DateTime,
                Parent = destinationDir,
                Name = destFileName
            };
            destinationDir.Files[copiedFile.Name] = copiedFile;
        }
    }

    public bool FileContentAreEqual(string firstPath, string secondPath)
    {
        if (firstPath == null || secondPath == null)
        {
            throw new ArgumentNullException();
        }
        
        var firstFile = GetFile(firstPath);
        if (firstFile == null)
        {
            throw new FileNotFoundException($"File {firstPath} not found");
        }
        var secondFile = GetFile(secondPath);
        if (secondFile == null)
        {
            throw new FileNotFoundException($"File {secondPath} not found");
        }

        if (firstFile.Content.Length != secondFile.Content.Length)
        {
            return false;
        }

        for (int i = 0; i < firstFile.Content.Length; i++)
        {
            if (firstFile.Content[i] != secondFile.Content[i])
            {
                return false;
            }
        }

        return true;
    }

    public void AddFile(string path, byte[] content, DateTime dateTime)
    {
        if (path == null)
        {
            throw new ArgumentNullException();
        }

        var parentDir = GetParentDirOfFilePath(path);
        if (parentDir == null)
        {
            throw new DirectoryNotFoundException($"directory of file {path} not found");
        }
        
        var fileName = GetFileName(path);

        if (parentDir.Files.ContainsKey(fileName))
        {
            throw new IOException($"file {path} already exists");
        }

        var file = new InMemoryFile
        {
            Content = content,
            DateTime = dateTime,
            Parent = parentDir,
            Name = fileName
        };
        parentDir.Files[fileName] = file;
    }
}