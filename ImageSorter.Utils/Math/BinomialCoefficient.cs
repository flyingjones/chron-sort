namespace ImageParser.Utils.Math;

public static class BinomialCoefficient
{
    public static long CalculateBinomialCoefficient(long n, long k)
    {
        // This function gets the total number of unique combinations based upon N and K.
        // N is the total number of items.
        // K is the size of the group.
        // Total number of unique combinations = N! / ( K! (N - K)! ).
        // This function is less efficient, but is more likely to not overflow when N and K are large.
        // Taken from:  http://blog.plover.com/math/choose.html
        //
        long r = 1;
        long d;
        if (n > k) return 0;
        for (d = 1; d <= k; d++)
        {
            r *= n--;
            r /= d;
        }
        return r;
    }
}