using System.CommandLine;
using System.Numerics;
using ImageSorter;

Console.WriteLine($"Vector<byte> size: {Vector<byte>.Count}");

Console.CursorVisible = false;
var rootCommand = RootCommandFactory.BuildRootCommand();
rootCommand.SetHandler(RootCommandHandler.Handle);
await rootCommand.InvokeAsync(args);

// TODO to error handling (e.g. catch some exceptions and print a semi pretty error message)

Thread.Sleep(50);