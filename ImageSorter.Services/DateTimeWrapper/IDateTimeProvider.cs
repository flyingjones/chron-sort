namespace ImageSorter.Services.DateTimeWrapper;

/// <summary>
/// Wrapper for static <see cref="DateTime"/> methods to make usages unit testable
/// </summary>
public interface IDateTimeProvider
{
    /// <inheritdoc cref="DateTime.Now"/>
    DateTime Now();

    /// <inheritdoc cref="DateTime.Today"/>
    DateTime Today();
}