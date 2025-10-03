namespace ImageSorter.Sorting.Tests.Services.EquivalenceClassFinder;

public class EquivalenceClassFinderTests
{
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(100)]
    public void GroupIntoEquivalenceClasses_DistinctItems_Works(int dataLength)
    {
        // arrange
        int comparisonCounter = 0;
        var comparisonFunc = (int a, int b) =>
        {
            comparisonCounter++;
            return a == b;
        };

        var items = Enumerable.Range(0, dataLength).ToArray();
        
        // act
        var service = new ImageSorter.Sorting.Services.EquivalenceClassFinder.EquivalenceClassFinder();
        var equivalenceClasses = service.GroupIntoEquivalenceClasses(
            items, 
            (a, b) => a == b, 
            comparisonFunc);
        
        // assert
        Assert.That(equivalenceClasses.Count, Is.EqualTo(items.Length));
        foreach (var equivalence in equivalenceClasses)
        {
            Assert.That(equivalence.Items.Count, Is.EqualTo(1));
        }

        // think of one triangle of a matrix without the diagonal
        var expectedNumberOfComparisons = (dataLength * dataLength - dataLength) / 2;
        Assert.That(comparisonCounter, Is.EqualTo(expectedNumberOfComparisons));
    }
    
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(100)]
    public void GroupIntoEquivalenceClasses_EqualvalentItems_Works(int dataLength)
    {
        // arrange
        int comparisonCounter = 0;
        var comparisonFunc = (int a, int b) =>
        {
            comparisonCounter++;
            return a % 2 == b % 2;
        };

        var items = Enumerable.Range(0, dataLength).Select(x => 2 * x).ToArray();
        
        // act
        var service = new ImageSorter.Sorting.Services.EquivalenceClassFinder.EquivalenceClassFinder();
        var equivalenceClasses = service.GroupIntoEquivalenceClasses(
            items, 
            (a, b) => a == b,
            comparisonFunc);
        
        // assert
        Assert.That(equivalenceClasses.Count, Is.EqualTo(1));
        var onlyClass = equivalenceClasses.First();
        Assert.That(onlyClass.Items.Count, Is.EqualTo(dataLength));

        // since all is equivalent, we should only need to compare each item once 
        var expectedNumberOfComparisons = dataLength - 1;
        Assert.That(comparisonCounter, Is.EqualTo(expectedNumberOfComparisons));
    }
    
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(100)]
    public void GroupIntoEquivalenceClasses_EqualItems_Works(int dataLength)
    {
        // arrange
        int comparisonCounter = 0;
        var comparisonFunc = (int a, int b) =>
        {
            comparisonCounter++;
            return a == b;
        };

        var items = Enumerable.Range(0, dataLength).Select(x => 1234).ToArray();
        
        // act
        var service = new ImageSorter.Sorting.Services.EquivalenceClassFinder.EquivalenceClassFinder();
        var equivalenceClasses = service.GroupIntoEquivalenceClasses(
            items, 
            (a, b ) => a == b,
            comparisonFunc);
        
        // assert
        Assert.That(equivalenceClasses.Count, Is.EqualTo(1));
        var onlyClass = equivalenceClasses.First();
        Assert.That(onlyClass.Items.Count, Is.EqualTo(dataLength));

        // since all is equal, we don't need to execute the equivalency function at all
        var expectedNumberOfComparisons = 0;
        Assert.That(comparisonCounter, Is.EqualTo(expectedNumberOfComparisons));
    }
}