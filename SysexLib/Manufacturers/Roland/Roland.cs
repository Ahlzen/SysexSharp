using Ahlzen.SysexSharp.SysexLib.Parsing;
using System;
using System.Collections.Generic;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Roland;

public sealed class RolandSysex : Sysex
{
    /// <remarks>
    /// More recent Roland devices follow the same pattern:
    /// 0xf0, 0x41, channel, modelID, commandID
    /// </remarks>
    private static readonly byte?[] RolandStandardHeader =
    {
        0xf0, // Start-of-sysex
        0x41, // Roland
        null, // Device id (channel) (usually 0-31; 0x7f - broadcast/all?)
        null, // Model id (may be multiple bytes)
        null, // Command id
    };

    private static readonly Dictionary<byte, string> RolandStandardCommandIds = new() {
        {0x11, "Data request"}, // one way transfer
        {0x12, "Data set"}, // one way transfer
        {0x40, "Send request"},
        {0x41, "Data request"}, // handshake mode
        {0x42, "Data set"}, // handshake mode
        {0x43, "Acknowledge"},
        {0x45, "End-of-data"},
        {0x4e, "Communication error"},
        {0x4f, "Rejection"},
    };

    private static readonly Dictionary<byte?[], string> RolandDeviceIds = new()
    {
        // Single-byte IDs
        { new byte?[] { 0x10 }, "S-10" }, // also: S-220, MKS-100
        { new byte?[] { 0x14 }, "D-50" },
        { new byte?[] { 0x16 }, "D-20" }, // also: MT-32, D-10, D-110
        { new byte?[] { 0x18 }, "S-50" },
        { new byte?[] { 0x1d }, "TR-626" },
        { new byte?[] { 0x1e }, "S-550" }, // also: S-330
        { new byte?[] { 0x28 }, "R-8" },
        { new byte?[] { 0x2B }, "U-110" }, // also: U-20, U-220
        { new byte?[] { 0x34 }, "S-770" },
        { new byte?[] { 0x39 }, "D-70" },
        { new byte?[] { 0x3a }, "MC-307" }, // listed in manual as "MC-307 Quick"
        { new byte?[] { 0x3d }, "JD-800" },
        { new byte?[] { 0x3d }, "JX-1" },
        { new byte?[] { 0x42 }, "GS" }, // used by several devices when in GS mode
        { new byte?[] { 0x45 }, "Display Data" }, // ? used to modify screen contents by e.g. SC-55, SC-88
        { new byte?[] { 0x46 }, "JV-1000" }, // also: JV-80, JV-90, JV-880
        { new byte?[] { 0x4d }, "JV-30" },
        { new byte?[] { 0x50 }, "R-70" },
        { new byte?[] { 0x53 }, "DJ-70" },
        { new byte?[] { 0x57 }, "JD-990" },
        { new byte?[] { 0x5e }, "R-8 MKII" },
        { new byte?[] { 0x6a }, "JV-1080" }, // also: JV-1010, JV-2080, XP-30, XP-50, XP-60, XP-80
        { new byte?[] { 0x7b }, "XP-10"},
        
        // Multi-byte IDs
        { new byte?[] { 0x00, 0x03 }, "MC-303" },
        { new byte?[] { 0x00, 0x06 }, "JP-8000" }, // also: JP-8080
        { new byte?[] { 0x00, 0x0b }, "JX-305" }, // also: MC-307, MC-505
        { new byte?[] { 0x00, 0x0d }, "D2" },
        { new byte?[] { 0x00, 0x10 }, "XV-3080" }, // also: XV-5050, XV-5080
        { new byte?[] { 0x00, 0x18 }, "EG-101" },
        { new byte?[] { 0x00, 0x1d }, "VP-9000" },
        { new byte?[] { 0x00, 0x4a }, "SH-32" },
        { new byte?[] { 0x00, 0x4f }, "MC-09" },
        { new byte?[] { 0x00, 0x53 }, "V-Synth" },
        { new byte?[] { 0x00, 0x59 }, "MC-909" },
        { new byte?[] { 0x00, 0x64 }, "Juno-D" }, // also: RS-50, RS-70
        { new byte?[] { 0x00, 0x6b }, "Fantom-S" }, // also: Fantom-S88, Fantom-X6, Fantom-X7, Fantom-X8
        { new byte?[] { 0x00, 0x00, 0x14 }, "MC-808" },
        { new byte?[] { 0x00, 0x00, 0x15 }, "Juno-G" },
        { new byte?[] { 0x00, 0x00, 0x16 }, "SH-201" },
        { new byte?[] { 0x00, 0x00, 0x25 }, "Juno-Stage" }, // also: SonicCell
        { new byte?[] { 0x00, 0x00, 0x3a }, "Juno-Di" }, // also: Juno-DS61, Juno-DS88
        { new byte?[] { 0x00, 0x00, 0x3b }, "VP-770" },
        { new byte?[] { 0x00, 0x00, 0x41 }, "Gaia SH-01" },
        { new byte?[] { 0x00, 0x00, 0x55 }, "Jupiter-80" },
        { new byte?[] { 0x00, 0x00, 0x00, 0x0F }, "JD-XA" },
        { new byte?[] { 0x00, 0x00, 0x00, 0x65 }, "Jupiter-X" }, // also: Jupiter-Xm

    };

    internal class LegacyRolandHeader
    {
        public byte?[] Pattern { get; }
        public int? Length { get; }
        public string? Device { get; }
        public string? Type { get; }

        public LegacyRolandHeader(byte?[] pattern, int? length, string? device, string? type) {
            Pattern = pattern;
            Length = length;
            Device = device;
            Type = type;
        }
    }

    private static readonly List<LegacyRolandHeader> LegacyRolandHeaders = new()
    {
        // TR-707 (also: TR-727, TR-909)
        // TODO: Used by other Roland devices as well?
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x50, 0xf7 }, 4, "TR-707", "Want to send file"), // (WSF)
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x51, 0xf7 }, 4, "TR-707", "Request file"), // (RQF)
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x52, 0x01 }, 519, "TR-909", "Data"), // (DAT) 01 format type = 909
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x52, 0x02 }, 519, "TR-707", "Data"), // (DAT) 02 format type = 707/727
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x53, 0xf7 }, 4, "TR-707", "Acknowledge"), // (PAS)
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x54, 0xf7 }, 4, "TR-707", "Continue"), // (CNT)
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x70, 0xf7 }, 4, "TR-909", "Abort"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x71, 0xf7 }, 4, "TR-909", "Error"),

        // Juno-106
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x30 }, 24, "Juno-106", "Patch data"), // also HS-60, MKS-7 (melody/chord/bass data)
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x31 }, 24, "Juno-106", "Manual mode"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x32 }, 7,  "Juno-106", "Control change"),

        // JX-8P:  [Start-of-sysex, Roland ID, Operation code, Unit#, JX-8P ID, Level, Group, ...]  TODO: Add lengths
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x31, 0x34, null, 0x21, 0x20, 0x01 }, null, "JX-8P", "Program number"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x35, null, 0x21, 0x20, 0x01 }, null, "JX-8P", "All tone parameters"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x36, null, 0x21, 0x20, 0x01 }, null, "JX-8P", "Individual tone parameter"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x35, null, 0x21, 0x30, 0x01 }, null, "JX-8P", "All patch parameters"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x31, 0x36, null, 0x21, 0x30, 0x01 }, null, "JX-8P", "Individual patch parameter"),

        // JX-10
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x40, null, 0x24 }, null, "JX-10", "Send request"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x41, null, 0x24 }, null, "JX-10", "Data request"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x42, null, 0x24 }, 135,  "JX-10", "Data"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x43, null, 0x24 }, 6,    "JX-10", "Acknowledge"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x45, null, 0x24 }, 6,    "JX-10", "End-of-file"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x4e, null, 0x24 }, 6,    "JX-10", "Communication error"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x4f, null, 0x24 }, 6,    "JX-10", "Rejection"),

        // Alpha Juno-1 [Start-of-sysex, Roland ID, Operation code, Unit#, Format type (Ju1/2), Level, Group, ...]
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x35, null, 0x23, 0x20, 0x01}, 54, "Alpha Juno-1", "All tone parameters"), // also Alpha Juno-2
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x35, null, 0x23, 0x20, 0x01}, 44, "Alpha Juno-1", "All tone parameters (without tone name)"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x36, null, 0x23, 0x20, 0x01}, null, "Alpha Juno-1", "Individual tone parameter"), // also Alpha Juno-2
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x37, null, 0x23, 0x20, 0x01}, null, "Alpha Juno-1", "Bulk dump"),

        // MKS-70
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x34, null, 0x24 }, 11,  "MKS-70", "Program number (patch)"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x34, null, 0x24 }, 11,  "MKS-70", "Program number (tone)"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x35, null, 0x24 }, 59,  "MKS-70", "Patch data"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x35, null, 0x24 }, 67,  "MKS-70", "Tone data"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x36, null, 0x24 }, 10,  "MKS-70", "Patch parameter"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x36, null, 0x24 }, 10,  "MKS-70", "Tone parameter"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x37, null, 0x24 }, 106, "MKS-70", "Patch bulk dump"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x37, null, 0x24 }, 69,  "MKS-70", "Tone bulk dump"),

        // MKS-80
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x36, null, 0x20, 0x20 }, null, "MKS-80", "Individual tone parameter(s)"), // a.k.a. IPR
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x36, null, 0x20, 0x30 }, null, "MKS-80", "Individual patch parameter(s)"), // a.k.a. IPR
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x35, null, 0x20, 0x20 }, 56, "MKS-80", "All tone parameters"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x35, null, 0x20, 0x30 }, 23, "MKS-80", "All patch parameters"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x34, null, 0x20, 0x30 }, 11, "MKS-80", "Program number"), // a.k.a. PGR (program/patch number)
    };

    public new string? Device { get; set; }
    public new string? Type { get; set; }
    public new bool IsKnownType { get; set; }

    public RolandSysex(byte[] data, string? name = null, int? expectedLength = null)
        : base(data, name, expectedLength)
    {
        // Sanity checks
        SanityCheck(data);
        if (ManufacturerName != "Roland")
            throw new ArgumentException("Data does not contain a Roland sysex", nameof(data));

        // Special cases ("legacy" headers)
        foreach (LegacyRolandHeader legacyHeader in LegacyRolandHeaders)
        {
            if (ParsingUtils.MatchesPattern(data, legacyHeader.Pattern))
            {
                if (legacyHeader.Length == null ||
                    legacyHeader.Length == data.Length)
                {
                    Device = legacyHeader.Device;
                    Type = legacyHeader.Type;
                    IsKnownType = true;
                    return;
                }
            }
        }

        // Standard Roland sysex format
        if (ParsingUtils.MatchesPattern(data, RolandStandardHeader))
        {
            foreach (byte?[] pattern in RolandDeviceIds.Keys)
            {
                if (ParsingUtils.MatchesPattern(data, pattern, 2))
                {
                    Device = RolandDeviceIds[pattern];
                    return;
                }
            }
        }
    }


}