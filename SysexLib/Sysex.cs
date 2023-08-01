using System;
using System.Diagnostics;

namespace Ahlzen.SysexSharp.SysexLib;

/// <summary>
/// Base class for all types of Sysex.
/// </summary>
/// <remarks>
/// This class can be used directly for generic Sysex data, or used
/// as a base class with selected members overridden for known specific
/// sysex types.
/// </remarks>
public class Sysex
{
    /// <summary>
    /// For all sysex types, Data contains the binary MIDI data
    /// that can be sent to or received from a device,
    /// including start and end-of-sysex markers.
    /// </summary>
    public byte[] Data { get; }

    /// <summary>
    /// The name of the sysex. This is usually set on parsing
    /// if the sysex contains a name (such as the name of an
    /// individual patch), or otherwise may be the filename
    /// of the sysex file, or null if neither applies.
    /// </summary>
    public virtual string? Name { get; }

    /// <summary>
    /// Manufacturer ID (1-3 bytes). Should be set for all known sysex types.
    /// E.g. [0x43] for Yamaha.
    /// </summary>
    public virtual byte[] ManufacturerId
        => Manufacturers.ManufacturerData.GetId(Data);

    /// <summary>
    /// Name of manufacturer, if known, derived from Manufacturer ID.
    /// E.g. "Yamaha".
    /// </summary>
    public virtual string? ManufacturerName
        => Manufacturers.ManufacturerData.GetName(ManufacturerId);

    /// <summary>
    /// Name of the device this sysex type is for, if known.
    /// E.g. "DX7".
    /// </summary>
    public virtual string? Device => null;

    /// <summary>
    /// Sysex type for this device (many devices support several
    /// different types of sysex), if known.
    /// E.g. "32-voice bank" 
    /// </summary>
    public virtual string? Type => null;

    /// <summary>
    /// Number of bytes in Sysex data, including including start
    /// and end-of-sysex markers
    /// </summary>
    public virtual int Length => Data.Length;

    /// <summary>
    /// True for all known sysex types. If true, you can expect
    /// at least a ManufacturerName, Device and Type to be set.
    /// May be false for unknown/generic sysex data.
    /// </summary>
    public virtual bool IsKnownType => !string.IsNullOrEmpty(Type);


    /// <param name="data">Raw binary data, including start/end-of-sysex bytes.</param>
    /// <param name="name">Filename, bank name etc, or null if not known/applicable.</param>
    /// <param name="expectedLength">If non-null, constructor checks that the supplied
    /// data is this many bytes and throws an ArgumentException otherwise.</param>
    public Sysex(byte[] data, string? name = null, int? expectedLength = null)
    {
        Debug.Assert(data != null);
        Debug.Assert(data.Length >= 3);

        // There may be more sysex markers if containing multiple
        // sub-sections, but the first and last byte should be
        // sysex markers.
        Debug.Assert(data[0] == Constants.START_OF_SYSEX);
        Debug.Assert(data[data.Length - 1] == Constants.END_OF_SYSEX);

        Data = data;
        Name = name;

        if (expectedLength != null)
        {
            if (data.Length != expectedLength)
            {
                throw new ArgumentException(
                    $"Data was not of the expected length. Expected: {expectedLength}, actual: {data.Length}.",
                    nameof(data));
            }
        }
    }
}
