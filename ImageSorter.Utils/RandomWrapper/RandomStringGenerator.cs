namespace ImageParser.Utils.RandomWrapper;

public class RandomStringGenerator : IRandomStringGenerator
{
    private readonly char[] _charset;
    private readonly Random _random = new Random();

    public RandomStringGenerator(char[] charset)
    {
        _charset = charset;
    }

    public static RandomStringGenerator CreateForLowerCaseLetters()
    {
        const string charSet = "abcdefghijklmnopqrstuvwxyz";
        return new RandomStringGenerator(charSet.ToCharArray());
    }

    public string GenerateRandomString(int length)
    {
        var stringChars = new char[length];

        for (var i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = _charset[_random.Next(_charset.Length)];
        }

        return new string(stringChars);
    }
}