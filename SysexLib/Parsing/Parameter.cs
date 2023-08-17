using System;
using System.Diagnostics;
using System.Text;

namespace Ahlzen.SysexSharp.SysexLib.Parsing;

/// <summary>
/// Contains information about why a parameter value is not
/// valid and, if possible, a corrected value that is within the
/// valid range for this parameter.
/// </summary>
public class ValidationException : Exception
{
    public object CorrectedValue { get; }
    public ValidationException(string message, object correctedValue) :
        base(message)
    {
        CorrectedValue = correctedValue;
    }
}


/// <summary>
/// Base class for known sysex parameters.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class Parameter
{
    public int Offset { get; }
    public string Name { get; }

    /// <param name="name">Parameter name (used for display and serialization)</param>
    /// <param name="offset">Offset (bytes) to start of parameter data</param>
    protected Parameter(int offset, string name)
    {
        Offset = offset;
        Name = name;
    }


    /// <summary>
    /// Validates that the specified value is valid
    /// for this parameter (length, range, etc)
    /// </summary>
    /// <exception cref="ValidationException">
    /// Thrown if the specified value is not within the allowable range.
    /// </exception>
    public abstract void Validate(object value);

    /// <summary>
    /// Validates that the current value is valid
    /// for this parameter (length, range, etc)
    /// </summary>
    /// <exception cref="ValidationException">
    /// Thrown if the specified value is not within the allowable range.
    /// </exception>
    public virtual void Validate(byte[] data)
        => Validate(GetValue(data));

    /// <summary>
    /// Returns this parameter's current value in the
    /// specified data. Does not validate the value.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown if the value cannot be determined, for example
    /// if the specified data is invalid or too short.
    /// </exception>
    /// <param name="data">The full sysex data.</param>
    /// <param name="extraOffset">Optional. Additional bytes to add to parameter's offset.</param>
    public abstract object GetValue(byte[] data, int extraOffset = 0);

    /// <summary>
    /// Updates this parameter's current value in the
    /// specified data. Does not validate the value.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown if the value cannot be set as specified, for example
    /// if the parameter extends beyond the length of the data.
    /// </exception>
    /// <param name="data">The full sysex data.</param>
    /// /// <param name="extraOffset">Optional. Additional bytes to add to parameter's offset.</param>
    public abstract void SetValue(byte[] data, object value, int extraOffset = 0);
}

/// <summary>
/// ASCII string parameter. Space (0x20) padded in data.
/// </summary>
public class AsciiParameter : Parameter
{
    public int Length { get; }

    
    /// <param name="length">String length, in bytes/characters</param>
    public AsciiParameter(int offset, string name, int length)
        : base(offset, name)
    {
        Length = length;
    }

    public override void Validate(object value)
    {
        if (value != null && value is not string)
            throw new ArgumentException($"Value is not of the right type. Expected: string, was: {value.GetType()}.");

        var s = value as string;
        if (s == null)
            throw new ValidationException("Value is null.", "");
        if (s.Length > this.Length)
            throw new ValidationException(
                "String is too long.", s.Substring(0,Length));
        if (ParsingUtils.HasNonPrintableCharacters(s))
            throw new ValidationException(
                "String contains non-printable characters.",
                ParsingUtils.ReplaceNonPrintableCharacters(s));
    }

    public override object GetValue(byte[] data, int extraOffset = 0)
    {
        int effectiveOffset = Offset + extraOffset;
        if (effectiveOffset + Length > data.Length)
            throw new ArgumentException(
                "String value extends beyond end of data.", nameof(data));
        return Encoding.ASCII.GetString(data, effectiveOffset, Length).TrimEnd();
    }
    public string GetString(byte[] data, int extraOffset = 0)
        => (GetValue(data, extraOffset) as string)!;

    public override void SetValue(byte[] data, object value, int extraOffset = 0)
    {
        int effectiveOffset = Offset + extraOffset;
        if (value != null && value is not string)
            throw new ArgumentException($"Value is not of the right type. Expected: string, was: {value.GetType()}.");

        var s = value as string;
        if (effectiveOffset + s.Length > data.Length)
            throw new ArgumentException(
                "String value extends beyond end of data.", nameof(value));
        if (s.Length > this.Length)
            throw new ArgumentException("String is too long.");
        Array.Fill<byte>(data, 0x20, effectiveOffset, Length); // space-pad
        Encoding.ASCII.GetBytes(s, 0, s.Length, data, effectiveOffset);
    }
    public void SetString(byte[] data, string value, int extraOffset = 0)
        => SetValue(data, value, extraOffset);
}

/// <summary>
/// Numeric (integer) parameter.
/// </summary>
public class NumericParameter : Parameter
{
    public int MinValue { get; }
    public int MaxValue { get; }
    public int BitCount { get; }
    public int BitOffset { get; }
        
    /// <param name="name">Parameter name (used for display and serialization)</param>
    /// <param name="offset">Offset in data (bytes)</param>
    /// <param name="minValue">Minimum allowed parameter value</param>
    /// <param name="maxValue">Maximum allowed parameter value</param>
    /// <param name="bitCount">(optional) Number of bits for parameter.</param>
    /// <param name="bitOffset">(optional) Parameter's bit offset (from right)</param>
    /// <remarks>
    /// See e.g. the Yamaha DX7 packed voice data format for examples
    /// of packed data with more than one parameter per MIDI-byte.
    /// </remarks>
    public NumericParameter(int offset, string name,
        int minValue = 0, int maxValue = 127,
        int bitCount = 7, int bitOffset = 0) : base(offset, name)
    {
        Debug.Assert(name != null);
        Debug.Assert(offset >= 0);
        Debug.Assert(maxValue >= minValue);
        Debug.Assert(bitCount > 0);
        Debug.Assert(bitOffset >= 0);

        MinValue = minValue;
        MaxValue = maxValue;
        BitCount = bitCount;
        BitOffset = bitOffset;
    }

    public override void Validate(object value)
    {
        if (value == null)
            throw new ValidationException("Value is null.", "");
        if (value is not int)
            throw new ArgumentException($"Value is not of the right type. Expected: int, was: {value.GetType()}.");

        int i = (int)value;
        if (i < MinValue)
            throw new ValidationException(
                "Value is too small.", MinValue);
        if (i > MaxValue)
            throw new ValidationException(
                "Value is too large.", MaxValue);
    }

    public override object GetValue(byte[] data, int extraOffset = 0)
    {
        int effectiveOffset = Offset + extraOffset;
        if (effectiveOffset >= data.Length)
            throw new ArgumentException(
                "Data is not long enough");

        int mask = (1 << BitCount) - 1;
        int value = ((int)data[effectiveOffset] >> BitOffset) & mask;
        return value;
    }

    public override void SetValue(byte[] data, object value, int extraOffset = 0)
    {
        if (value == null)
            throw new ValidationException("Value is null.", "");
        if (value is not int)
            throw new ArgumentException($"Value is not of the right type. Expected: int, was: {value.GetType()}.");

        int i = (int)value;
        int effectiveOffset = Offset + extraOffset;
        if (effectiveOffset >= data.Length)
            throw new ArgumentException(
                "Data is not long enough");

        int srcMask = (1 << BitCount) - 1;
        int dstMask = 0xff - (srcMask << BitOffset);
        i = (i << BitOffset) & srcMask;
        data[effectiveOffset] &= (byte)dstMask; // clear parameter's bit in byte
        data[effectiveOffset] |= (byte)i; // set parameter value
    }
}

