namespace ImageSorter.Tests.InMemory;

public class InMemoryFile
{
    public required byte[] Content { get; set; }
    
    public required string Name { get; set; }
    
    public required DateTime DateTime { get; set; }
    
    public required InMemoryDirectory Parent { get; set; }
    
    public string Path => $"{Parent.Path}{System.IO.Path.DirectorySeparatorChar}{Name}";
}