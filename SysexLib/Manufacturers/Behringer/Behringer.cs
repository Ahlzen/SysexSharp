using Ahlzen.SysexSharp.SysexLib.Parsing;
using System;
using System.Collections.Generic;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Behringer;

public class BehringerSysex : Sysex
{
    private static readonly Dictionary<byte?[], string> BehringerHeaders = new()
    {
        // From https://github.com/samstaton/pro800/tree/main
        { new byte?[] { 0xf0, 0x00, 0x20, 0x32, 0x00, 0x01, 0x24, 0x00 }, "Pro-800" }
    };

    public new string? Device { get; protected set; }
    public new string? Type { get; protected set; }

    public BehringerSysex(byte[] data, string? name = null, int? expectedLength = null)
        : base(data, name, expectedLength)
    {
        // Sanity checks
        if (ManufacturerName != "Behringer")
            throw new ArgumentException("Data is not a valid Behringer sysex", nameof(data));

        foreach (KeyValuePair<byte?[], string> header in BehringerHeaders)
        {
            if (ParsingUtils.MatchesPattern(data, header.Key))
            {
                Device = header.Value;
                return;
            }
        }
    }

}
