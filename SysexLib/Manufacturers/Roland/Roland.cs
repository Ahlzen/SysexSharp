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
        null, // Device id / channel (0-15)
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

    private static readonly Dictionary<byte?[], string> RolandModelIds = new()
    {
        // Single-byte IDs
        { new byte?[] { 0x14 }, "D-50" },
        { new byte?[] { 0x16 }, "D-20" }, // also: MT-32?
        { new byte?[] { 0x3d }, "JD-800" },
        { new byte?[] { 0x42 }, "GS" }, // used by several devices when in GS mode
        { new byte?[] { 0x6a }, "JV-1080" }, // also: JV-1010/2080/XP-80

        // Multi-byte IDs
        { new byte?[] { 0x00, 0x06 }, "JP-8000" },
    };

    internal class LegacyRolandHeader
    {
        public byte?[] Data { get; }
        public int? Length { get; }
        public string? Device { get; }
        public string? Type { get; }

        public LegacyRolandHeader(byte?[] data, int? length, string? device, string? type) {
            Data = data;
            Length = length;
            Device = device;
            Type = type;
        }
    }

    private static readonly List<LegacyRolandHeader> LegacyRolandHeaders = new()
    {
        // Juno-106
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x30 }, 24, "Juno-106", "Patch data"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x31 }, 24, "Juno-106", "Manual mode"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x32 }, 7,  "Juno-106", "Control change"),

        // JX-10
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x40, null, 0x24 }, null, "JX-10", "Send request"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x41, null, 0x24 }, null, "JX-10", "Data request"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x42, null, 0x24 }, 135,  "JX-10", "Data"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x43, null, 0x24 }, 6,    "JX-10", "Acknowledge"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x45, null, 0x24 }, 6,    "JX-10", "End-of-file"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x4e, null, 0x24 }, 6,    "JX-10", "Communication error"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x4f, null, 0x24 }, 6,    "JX-10", "Rejection"),

        // MKS-70
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x34, null, 0x24 }, 11,  "MKS-70", "Program number (patch)"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x34, null, 0x24 }, 11,  "MKS-70", "Program number (tone)"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x35, null, 0x24 }, 59,  "MKS-70", "Patch data"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x35, null, 0x24 }, 67,  "MKS-70", "Tone data"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x36, null, 0x24 }, 10,  "MKS-70", "Patch parameter"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x36, null, 0x24 }, 10,  "MKS-70", "Tone parameter"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x37, null, 0x24 }, 106, "MKS-70", "Patch bulk dump"),
        new LegacyRolandHeader(new byte?[] { 0xf0, 0x41, 0x37, null, 0x24 }, 69,  "MKS-70", "Tone bulk dump"),
    };

    public RolandSysex(byte[] data, string? name = null, int? expectedLength = null)
        : base(data, name, expectedLength)
    {
        // Sanity checks
        SanityCheck(data);
        if (ManufacturerName != "Roland")
            throw new ArgumentException("Data does not contain a Roland sysex", nameof(data));

        // 
    }


}