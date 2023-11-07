using System;
using System.Diagnostics;
using Ahlzen.SysexSharp.SysexLib.Utils;

namespace Ahlzen.SysexSharp.SysexLib;

/// <summary>
/// Immutable base class for all types of Sysex.
/// </summary>
/// <remarks>
/// This class can be used directly for generic Sysex data, or used
/// as a base class with selected members overridden for known specific
/// sysex types.
/// 
/// Use SysexFactory to parse and initialize Sysex objects from data or file.
/// </remarks>
public class Sysex
{
    /// <summary>
    /// Raw binary MIDI data that can be sent to or received from a device,
    /// including start and end-of-sysex markers.
    /// </summary>
    protected byte[] _data;

    /// <summary>
    /// Number of bytes in Sysex data, including including start
    /// and end-of-sysex markers
    /// </summary>
    public virtual int Length => _data.Length;

    /// <summary>
    /// Returns the raw sysex data.
    /// </summary>
    /// <remarks>
    /// Always creates a copy of the data to ensure immutability
    /// (never return a reference to our internal buffer)
    /// </remarks>
    public virtual byte[] GetData() => _data.Copy();

    /// <summary>
    /// The name of the sysex. This is usually set on parsing
    /// if the sysex contains a name (such as the name of an
    /// individual patch), or otherwise may be the filename
    /// of the sysex file, or null if neither applies.
    /// </summary>
    public virtual string? Name { get; private set; }

    /// <summary>
    /// Manufacturer ID (1-3 bytes). Should be set for all known sysex types.
    /// E.g. [0x43] for Yamaha.
    /// </summary>
    public virtual byte[] ManufacturerId
        => Manufacturers.ManufacturerData.GetId(_data);

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

    

    /// <param name="data">Raw binary data, including start/end-of-sysex bytes.</param>
    /// <param name="name">Filename, bank name etc, or null if not known/applicable.</param>
    /// <param name="expectedLength">If non-null, constructor checks that the supplied
    /// data is this many bytes and throws an ArgumentException otherwise.</param>
    /// <remarks>
    /// Use SysexFactory to parse and initialize Sysex objects from data or file.
    /// </remarks>
    internal Sysex(byte[] data, string? name = null, int? expectedLength = null)
    {
        Init(data, name, expectedLength);
    }

    internal Sysex(Func<byte[]> dataFactory, string? name = null, int? expectedLength = null)
    {
        byte[] data = dataFactory();
        Init(data, name, expectedLength);
    }

    protected void Init(byte[] data, string? name = null, int? expectedLength = null)
    {
        SanityCheck(data);

        if (expectedLength.HasValue && expectedLength != data.Length)
            throw new ArgumentException(
                $"Data was not of the expected length. Expected: {expectedLength}, actual: {data.Length}.",
                nameof(data));

        Name = name;

        // Copy the provided data to ensure immutability
        _data = data.Copy();
    }

    /// <summary>
    /// Tests the specified data for being a valid sysex.
    /// Known sysex types implements more specific tests to verify
    /// that the data is of the specified type.
    /// </summary>
    /// <returns>True if the data appears to contain a valid sysex.</returns>
    public static bool Test(byte[] data)
    {
        try {
            Sysex.SanityCheck(data);
        }
        catch (ArgumentException) {
            return false;
        }
        return true;
    }


    /// <summary>
    /// Performs basic checks to ensure that the data is a (plausibly)
    /// valid sysex.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if data is not valid.</exception>
    internal static void SanityCheck(byte[] data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data),
                "Sysex data is null.");

        // There must be at least some data in addition to the sysex
        // markers and manufacturer's byte(s). So minimum is 4 bytes, but realistically
        // there should be more
        if (data.Length < 4)
            throw new ArgumentException(
                $"Sysex data is too short ({data.Length} bytes)", nameof(data));

        // There may be more sysex markers if containing multiple
        // sub-sections, but the first and last byte should always be
        // sysex markers.
        if (data[0] != Constants.START_OF_SYSEX)
            throw new ArgumentException(
                "Sysex data does not begin with start-of-sysex marker (f0)", nameof(data));
        if (data[^1] != Constants.END_OF_SYSEX)
            throw new ArgumentException(
                "Sysex data does not end with end-of-sysex marker (f7)", nameof(data));
    }
}
