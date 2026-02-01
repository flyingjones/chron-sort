using ImageSorter.DateParsing.Abstractions.Services;
using ImageSorter.DependencyInjection;
using ImageSorter.FileWrapper.Abstractions.Directory;
using ImageSorter.FileWrapper.Abstractions.File;
using ImageSorter.FileWrapper.Abstractions.FileStream;
using ImageSorter.Markdown.Abstractions.Services;
using ImageSorter.Markdown.Services;
using ImageSorter.Tests.InMemory;
using ImageSorter.Tests.InMemory.ServiceImpl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ImageSorter.Tests.Integration;

public static class IntegrationTestSetupHelper
{
    public static IServiceProvider BuildIntegrationTestServices(
        RunConfiguration runConfiguration,
        InMemoryFileSystem inMemoryFileSystem)
    {
        // mock fs is case-sensitive
        runConfiguration.FileSystemIsCaseSensitive = true;

        // set up the service provider
        var serviceProvider = runConfiguration
            .SetupServices()
            .AddSingleton<IMarkdownFileWriter>(new MockMarkdownFileWriter())
            .Replace(new ServiceDescriptor(typeof(IDateParser), new InMemoryDateParser(inMemoryFileSystem)))
            .Replace(new ServiceDescriptor(typeof(IDirectoryWrapper), new InMemoryDirectoryWrapper(inMemoryFileSystem)))
            .Replace(new ServiceDescriptor(typeof(IFileStreamService),
                new InMemoryFileStreamService(inMemoryFileSystem)))
            .Replace(new ServiceDescriptor(typeof(IFileWrapper), new InMemoryFileWrapper(inMemoryFileSystem)))
            .BuildServiceProvider();

        return serviceProvider;
    }
}