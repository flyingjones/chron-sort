namespace ImageSorter.ResultWriting.Abstractions;

public class ResultWritingException : Exception
{
    public ResultWritingException(string? message) : base(message)
    {
    }
}