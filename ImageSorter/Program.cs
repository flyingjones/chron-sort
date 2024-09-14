// See https://aka.ms/new-console-template for more information


using System.CommandLine;
using ImageSorter;

var rootCommand = RootCommandFactory.BuildRootCommand();
rootCommand.SetHandler(RootCommandHandler.Handle);
await rootCommand.InvokeAsync(args);