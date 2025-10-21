namespace ImageSorter.Test.Utils;

public abstract class AbstractTestCase
{
    public string? TestCaseName { get; set; }

    public override string ToString()
    {
        return $"{nameof(TestCaseName)}: {TestCaseName}";
    }
}