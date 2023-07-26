using System.Diagnostics;

namespace Ahlzen.SysexSharp.SysexLib;

/// <summary>
/// Base class for all types of Sysex.
/// </summary>
public abstract class Sysex
{
    /// <summary>
    /// For all sysex types, Data contains the binary MIDI data
    /// that can be sent to or received from a device,
    /// including start and end-of-sysex markers.
    /// </summary>
    public byte[] Data { get; internal set; }

    /// <summary>
    /// The name of the sysex. This is usually set on parsing
    /// if the sysex contains a name (such as the name of an
    /// individual patch), or otherwise may be the filename
    /// of the sysex file, or null if neither applies.
    /// </summary>
    public abstract string? Name { get; }

    /// <summary>
    /// Manufacturer ID (1-3 bytes). Should be set for all known sysex types.
    /// E.g. [0x43] for Yamaha.
    /// </summary>
    public abstract byte[] ManufacturerId { get; }

    /// <summary>
    /// Name of manufacturer, if known, derived from Manufacturer ID.
    /// E.g. "Yamaha".
    /// </summary>
    public abstract string? ManufacturerName { get; }

    /// <summary>
    /// Name of the device this sysex type is for, if known.
    /// E.g. "DX7".
    /// </summary>
    public abstract string? Device { get; }

    /// <summary>
    /// Sysex type for this device (many devices support several
    /// different types of sysex), if known.
    /// E.g. "32-voice bank" 
    /// </summary>
    public abstract string? Type { get; }


    /// <summary>
    /// Number of bytes in Sysex data, including including start
    /// and end-of-sysex markers
    /// </summary>
    public int Length => Data.Length;

    /// <summary>
    /// True for all known sysex types. If true, you can expect
    /// at least a ManufacturerName, Device and Type to be set.
    /// May be false for unknown/generic sysex data.
    /// </summary>
    public bool IsKnownType => !string.IsNullOrEmpty(Type);

    /// <summary>
    /// True for sysex types that can be parsed into
    /// into individual parameter data.
    /// If true, implements the following:
    /// - GetParameterNames()
    /// - GetParameterValue()
    /// - Validate()
    /// - ToJSON()
    /// - FromJSON()
    /// </summary>
    public abstract bool CanParse { get; }

    /// <summary>
    /// True in sysex types that contain sub-items
    /// that can be extracted and/or sent as individual sysex.
    /// If true, implements the following:
    /// - GetSubItemCount()
    /// - GetSubItemNames()
    /// - GetSubItem()
    /// - SetSubItem()
    /// </summary>
    public abstract bool HasSubItems { get; }


    internal Sysex(byte[] rawData)
    {
        Debug.Assert(rawData != null);
        Debug.Assert(rawData.Length >= 3);

        // There may be more sysex markers if containing multiple
        // sub-sections, but the first and last byte should be
        // sysex markers.
        Debug.Assert(rawData[0] == Constants.START_OF_SYSEX);
        Debug.Assert(rawData[rawData.Length - 1] == Constants.END_OF_SYSEX);

        Data = rawData;
    }
}
