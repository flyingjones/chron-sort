using ImageSorter.DependencyInjection;
using ImageSorter.Markdown.Abstractions.Services;
using ImageSorter.Markdown.Services;
using ImageSorter.Services;
using ImageSorter.Sorting.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImageSorter.Tests.DependencyInjection;

[TestFixture]
public class DependencySetupHelperTests
{
    [TestCaseSource(nameof(GetTestCases))]
    public void SetupServices_NoMissingDependencies(DependencySetupHelperTestCase testCase)
    {
        // arrange
        var serviceCollection = testCase.ToRunConfiguration().SetupServices();
        // initialized in the main and not the normal service stuff
        serviceCollection.AddSingleton<IMarkdownFileWriter, MockMarkdownFileWriter>();
        
        // act & assert
        Assert.DoesNotThrow(() =>
        {
            var serviceProvider = serviceCollection.BuildServiceProvider();
            
            // top level dependencies which are constructed in the main
            var logger = serviceProvider.GetRequiredService<ILogger<DependencySetupHelperTests>>();
            var markDownRenderEngine = serviceProvider.GetRequiredService<IMarkdownTableRenderEngine>();
            var sorter = serviceProvider.GetRequiredService<ISorter>();
        });
    }
    
    private static IEnumerable<DependencySetupHelperTestCase> GetTestCases()
    {
        yield return new DependencySetupHelperTestCase
        {
            IsDryRun = false,
            MoveFiles = false,
            ScanParallel = false,
            UseProgressBar = false,
            DestinationConflictMode = DestinationConflictMode.Skip,
            ConflictReducerMode = ConflictReducerMode.None,
            ConflictResolverMode = ConflictResolverMode.Throw
        };
        yield return new DependencySetupHelperTestCase
        {
            IsDryRun = true,
            MoveFiles = true,
            ScanParallel = true,
            UseProgressBar = true,
            DestinationConflictMode = DestinationConflictMode.Overwrite,
            ConflictReducerMode = ConflictReducerMode.FileLength,
            ConflictResolverMode = ConflictResolverMode.ChooseOne
        };
        yield return new DependencySetupHelperTestCase
        {
            IsDryRun = false,
            MoveFiles = false,
            ScanParallel = false,
            UseProgressBar = true,
            DestinationConflictMode = DestinationConflictMode.Joint,
            ConflictReducerMode = ConflictReducerMode.FileContent,
            ConflictResolverMode = ConflictResolverMode.RandomRename
        };
        yield return new DependencySetupHelperTestCase
        {
            IsDryRun = false,
            MoveFiles = false,
            ScanParallel = false,
            UseProgressBar = true,
            DestinationConflictMode = DestinationConflictMode.Joint,
            ConflictReducerMode = ConflictReducerMode.FileContent,
            ConflictResolverMode = ConflictResolverMode.HashRename
        };
        yield return new DependencySetupHelperTestCase
        {
            IsDryRun = false,
            MoveFiles = false,
            ScanParallel = false,
            UseProgressBar = true,
            DestinationConflictMode = DestinationConflictMode.Joint,
            ConflictReducerMode = ConflictReducerMode.FileContent,
            ConflictResolverMode = ConflictResolverMode.SemanticRename
        };
    }
}