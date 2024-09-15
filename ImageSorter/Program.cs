using System.CommandLine;
using ImageSorter;

var rootCommand = RootCommandFactory.BuildRootCommand();
rootCommand.SetHandler(RootCommandHandler.Handle);
await rootCommand.InvokeAsync(args);