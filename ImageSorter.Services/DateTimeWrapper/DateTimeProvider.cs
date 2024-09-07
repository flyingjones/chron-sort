namespace ImageSorter.Services.DateTimeWrapper;

/// <inheritdoc cref="IDateTimeProvider"/>
public class DateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc cref="IDateTimeProvider.Now"/>
    public DateTime Now()
    {
        return DateTime.Now;
    }

    /// <inheritdoc cref="IDateTimeProvider.Today"/>
    public DateTime Today()
    {
        return DateTime.Today;
    }
}