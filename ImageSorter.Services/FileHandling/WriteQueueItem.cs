namespace ImageSorter.Services.FileHandling;

public class WriteQueueItem
{
    public required string FilePath { get; set; }
    
    public required DateTime DateTaken { get; set; }
}