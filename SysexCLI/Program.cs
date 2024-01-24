using System.CommandLine;
using Ahlzen.SysexSharp.SysexLib;

namespace SysexCLI;

internal class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand(
            description: "Identifies, lists, extracts and modifies System Exclusive (sysex) MIDI messages.");

        var fileArgument = new Argument<string>(
            name: "filename",
            description: "Name of Sysex file (usually .syx)");

        var infoCommand = new Command("info", "Identify and display details about the specified file.");
        infoCommand.AddArgument(fileArgument);
        infoCommand.SetHandler((filename) => ShowInfo(filename), fileArgument);
        rootCommand.AddCommand(infoCommand);

        var extractCommand = new Command("extract", "Extract (save) individual items (patches, programs, etc) from the specified sysex bank.");
        extractCommand.AddArgument(fileArgument);
        extractCommand.SetHandler((filename) => Extract(filename), fileArgument);
        rootCommand.AddCommand(extractCommand);

        return await rootCommand.InvokeAsync(args);
    }

    private static void ShowInfo(string filePath)
    {
        Sysex? sysex = GetSysex(filePath);
        if (sysex == null) return;

        Console.WriteLine("File: " + filePath);
        Console.WriteLine("Manufacturer: " + sysex.ManufacturerName);
        if (sysex.Device != null)
            Console.WriteLine("Device: " + sysex.Device);
        if (sysex.Type != null)
            Console.WriteLine("Type: " + sysex.Type);

        if (sysex is IHasItems sysexBank)
        {
            Console.WriteLine();
            Console.WriteLine($"{sysexBank.ItemCount} items:");
            for (int i = 0; i < sysexBank.ItemCount; i++)
                Console.WriteLine($"* {i+1} - {sysexBank.GetItem(i).Name}");
        }
    }

    private static void Extract(string filePath)
    {
        Sysex? sysex = GetSysex(filePath);
        if (sysex == null) return;

        Console.WriteLine("File: " + filePath);
        if (sysex is IHasItems sysexBank)
        {
            Console.WriteLine($"{sysexBank.ItemCount} items:");
            for (int i = 0; i < sysexBank.ItemCount; i++)
            {
                Sysex patch = sysexBank.GetItem(i);
                string patchName = patch.Name ?? "";
                string destinationName = sysex.Name + " - " + (i+1) + " - " + patchName + ".syx";
                Console.WriteLine($"Saving {destinationName}...");
                File.WriteAllBytesAsync(destinationName, patch.GetData());
            }
        }
        else
        {
            Console.Error.WriteLineAsync(
                "Extracting items is not supported for this type of Sysex.");
        }
    }

    private static Sysex? GetSysex(string path)
    {
        Action<string?> err = Console.Error.WriteLine;
        if (!File.Exists(path))
        {
            err($"File {path} not found.");
            return null;
        }

        Sysex sysex;
        try
        {
            sysex = SysexFactory.Load(path);
        }
        catch (Exception ex)
        {
            err($"Failed to load file {path}: {ex.Message}");
            return null;
        }

        return sysex;
    }
}