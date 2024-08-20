namespace ImageSorter;

public class WriteQueueItem
{
    public required string FilePath { get; set; }
    
    public DateTime DateTaken { get; set; }
}