using ImageParser.Utils.Math;

namespace ImageSorter.Utils.Tests;

[TestFixture]
public class BinomialCoefficientTests
{
    [TestCase(0, 0, ExpectedResult = 1)]
    [TestCase(1, 0, ExpectedResult = 1)]
    [TestCase(1, 1, ExpectedResult = 1)]
    [TestCase(2, 0, ExpectedResult = 1)]
    [TestCase(2, 1, ExpectedResult = 2)]
    [TestCase(2, 2, ExpectedResult = 1)]
    [TestCase(3, 0, ExpectedResult = 1)]
    [TestCase(3, 1, ExpectedResult = 3)]
    [TestCase(3, 2, ExpectedResult = 3)]
    [TestCase(3, 3, ExpectedResult = 1)]
    [TestCase(49,6, ExpectedResult = 13_983_816)]
    [TestCase(0, 1, ExpectedResult = 0)]
    public long CalculateBinomialCoefficient(long n, long k)
    {
        return BinomialCoefficient.CalculateBinomialCoefficient(n, k);
    }
}