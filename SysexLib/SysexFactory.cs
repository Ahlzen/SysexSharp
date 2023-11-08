﻿using System.IO;
using Ahlzen.SysexSharp.SysexLib.Manufacturers.MIDI;
using Ahlzen.SysexSharp.SysexLib.Manufacturers.Roland;
using Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;

namespace Ahlzen.SysexSharp.SysexLib;

public static class SysexFactory
{
    public static Sysex Load(string filePath)
    {
        byte[] data = File.ReadAllBytes(filePath);
        string filename = Path.GetFileNameWithoutExtension(filePath);
        return Create(data, filename);
    }

    public static Sysex Create(byte[] data, string? filename = null)
    {
        Sysex.SanityCheck(data);
        byte[] manufacturerId = Manufacturers.Manufacturers.GetId(data);
        string? manufacturerName = Manufacturers.Manufacturers.GetName(manufacturerId);

        switch (manufacturerName)
        {
            case "MIDI": // Universal sysex types
                return new UniversalSysex(data);
            case "Roland":
                return new RolandSysex(data, filename);
            case "Yamaha":
                return YamahaFactory.Create(data, filename);
            case "Korg":
                return new Sysex(data, filename); // TODO

            // Add additional manufacturers with detailed support here

            default:
                return new Sysex(data, filename);
        }
    }
}
