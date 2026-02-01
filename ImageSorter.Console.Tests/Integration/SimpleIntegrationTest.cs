using ImageSorter.DependencyInjection;
using ImageSorter.FileHandling.CaseSensitivity;
using ImageSorter.Services;
using ImageSorter.Sorting.Model;
using ImageSorter.Tests.InMemory;
using Microsoft.Extensions.DependencyInjection;

namespace ImageSorter.Tests.Integration;

[TestFixture]
public class SimpleIntegrationTest
{
    [Test]
    public async Task SimpleTest()
    {
        // arrange
        var fs = new InMemoryFileSystem();

        fs.CreateDirectory($"{InMemoryFileSystem.RootDirName}/images/share");
        fs.CreateDirectory($"{InMemoryFileSystem.RootDirName}/images/phone");
        var filesToAdd = new[]
        {
            new KeyValuePair<string, DateTime>($"{InMemoryFileSystem.RootDirName}/images/share/img01.jpg", DateTime.Parse("2025-01-01")),
            new KeyValuePair<string, DateTime>($"{InMemoryFileSystem.RootDirName}/images/phone/img02.jpg", DateTime.Parse("2025-02-01"))
        };
        AddEmptyFiles(fs, filesToAdd);

        var runConfig = new RunConfiguration
        {
            SortConfiguration = [],
            PreferFileNameParsing = false,
            SourcePath = $"{InMemoryFileSystem.RootDirName}{Path.DirectorySeparatorChar}images",
            DestinationPath = $"{InMemoryFileSystem.RootDirName}{Path.DirectorySeparatorChar}out",
            SummaryFilePath = null,
            SummaryFileDirectoryPath = null,
            EscapeSummaryFileTables = false,
            MoveFiles = false,
            ScanParallel = false,
            SkipParserBefore = default,
            SkipParserAfter = default,
            IsDryRun = false,
            OutputFormat = "yyyy/MM/dd",
            UseProgressBar = false,
            ProgressBarCharacters = "#",
            ConflictReducerMode = ConflictReducerMode.None,
            DestinationConflictMode = DestinationConflictMode.Joint,
            ConflictResolverMode = ConflictResolverMode.Throw,
            CaseSensitivityDetectionMode = CaseSensitivityDetectionMode.Sensitive
        };
        var serviceProvider = IntegrationTestSetupHelper.BuildIntegrationTestServices(runConfig, fs);

        var sorter = serviceProvider.GetRequiredService<ISorter>();
        
        // act
        await sorter.PerformSorting(CancellationToken.None);
        
        // assert
        var resultingFiles = fs.GetFiles("/out", "*", SearchOption.AllDirectories);
        Assert.That(resultingFiles, Has.Length.EqualTo(2));
        Assert.That(resultingFiles, Contains.Item($"{InMemoryFileSystem.RootDirName}{Path.DirectorySeparatorChar}out{Path.DirectorySeparatorChar}2025{Path.DirectorySeparatorChar}01{Path.DirectorySeparatorChar}01{Path.DirectorySeparatorChar}img01.jpg"));
        Assert.That(resultingFiles, Contains.Item($"{InMemoryFileSystem.RootDirName}{Path.DirectorySeparatorChar}out{Path.DirectorySeparatorChar}2025{Path.DirectorySeparatorChar}02{Path.DirectorySeparatorChar}01{Path.DirectorySeparatorChar}img02.jpg"));
    }

    private static void AddEmptyFiles(
        InMemoryFileSystem fs,
        ICollection<KeyValuePair<string, DateTime>> filesWithDate)
    {
        foreach (var pair in filesWithDate)
        {;
            fs.AddFile(pair.Key, [], pair.Value);
        }
    }
}