namespace ImageParser.Utils.Math;

public static class BinomialCoefficient
{
    /// <summary>
    /// Calculates the <see href="https://en.wikipedia.org/wiki/Binomial_coefficient">Binomial Coefficient</see>
    /// </summary>
    /// <remarks>
    /// The binomial coefficient is defined as <c>N! / ( K! (N - K)! )</c>.
    /// </remarks>
    public static long CalculateBinomialCoefficient(long n, long k)
    {
        // This implementation uses the multiplicative formula since it is less likely to overflow for large values:
        // see https://en.wikipedia.org/wiki/Binomial_coefficient#Multiplicative_formula
        if (k > n) return 0;
        long result = 1;
        for (long i = 1; i <= k; i++)
        {
            result *= n--;
            result /= i;
        }
        return result;
    }
}