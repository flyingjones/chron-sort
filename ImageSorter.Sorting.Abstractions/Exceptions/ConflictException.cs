namespace ImageSorter.Sorting.Abstractions.Exceptions;

public class ConflictException : Exception
{
    public ConflictException() : base("Conflicts Found")
    {
    }
}