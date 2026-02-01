namespace ImageSorter.Tests.InMemory;

public class InMemoryDirectory
{
    public InMemoryDirectory(InMemoryDirectory? parent, string name)
    {
        Name = name;
        Parent = parent;
    }

    public Dictionary<string, InMemoryFile> Files { get; } = 
        new Dictionary<string, InMemoryFile>();

    public Dictionary<string, InMemoryDirectory> Directories { get; } =
        new Dictionary<string, InMemoryDirectory>();

    public InMemoryDirectory? Parent { get; set; }

    public bool IsEmpty => Files.Keys.Count == 0 && Directories.Keys.Count == 0;
    
    public string Name { get; }

    public void AddDirectory(string name)
    {
        if (Directories.ContainsKey(name) || Files.ContainsKey(name))
        {
            throw new IOException("File or directory already exists");
        }
        
        var newDir = new InMemoryDirectory(this, name);
        Directories[name] = newDir;
    }

    public string Path => Parent == null ? $"{Name}" : $"{Parent.Path}{System.IO.Path.DirectorySeparatorChar}{Name}";
}