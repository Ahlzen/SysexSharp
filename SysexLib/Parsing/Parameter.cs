using System;
using System.Diagnostics;
using System.Text;

namespace Ahlzen.SysexSharp.SysexLib.Parsing;

/// <summary>
/// Contains information about why a parameter value is not
/// valid and, if possible, a corrected value that is within the
/// valid range for this parameter.
/// </summary>
public class ValidationException<T> : Exception
{
    public T CorrectedValue { get; }
    public ValidationException(string message, T correctedValue) :
        base(message)
    {
        CorrectedValue = correctedValue;
    }
}

public abstract class Parameter<T>
{
    /// <summary>
    /// Validates that the specified value is valid
    /// for this parameter (length, range, etc)
    /// </summary>
    /// <exception cref="ValidationException">
    /// Thrown if the specified value is not within the allowable
    /// range.
    /// </exception>
    public abstract void Validate(T value);

    /// <summary>
    /// Returns this parameter's current value in the
    /// specified data. Does not validate the value.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown if the value cannot be determined, for example
    /// if the specified data is invalid or too short.
    /// </exception>
    /// <param name="data">The full sysex data.</param>
    public abstract T GetValue(byte[] data);

    /// <summary>
    /// Updates this parameter's current value in the
    /// specified data. Does not validate the value.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown if the value cannot be set as specified, for example
    /// if the parameter extends beyond the length of the data.
    /// </exception>
    /// <param name="data">The full sysex data.</param>
    public abstract void SetValue(byte[] data, T value);
}

/// <summary>
/// ASCII string parameter. Space (0x20) padded in data.
/// </summary>
public class AsciiParameter : Parameter<string>
{
    public string Name { get; }
    public int Offset { get; }
    public int Length { get; }

    /// <param name="name">Parameter name (used for display and serialization)</param>
    /// <param name="offset">Offset in data (bytes) of first character</param>
    /// <param name="length">String length, in bytes/characters</param>
    public AsciiParameter(string name, int offset, int length) {
        Name = name;
        Offset = offset;
        Length = length;
    }

    public override void Validate(string value)
    {
        if (value == null)
            throw new ValidationException<string>("Value is null.", "");
        if (value.Length > this.Length)
            throw new ValidationException<string>(
                "String is too long.", value.Substring(0,Length));
        if (ParsingUtils.HasNonPrintableCharacters(value))
            throw new ValidationException<string>(
                "String contains non-printable characters.",
                ParsingUtils.ReplaceNonPrintableCharacters(value));
    }

    public override string GetValue(byte[] data)
    {
        if (Offset + Length > data.Length)
            throw new ArgumentException(
                "String value extends beyond end of data.", nameof(data));
        return Encoding.ASCII.GetString(data, Offset, Length).TrimEnd();
    }

    public override void SetValue(byte[] data, string value)
    {
        if (Offset + value.Length > data.Length)
            throw new ArgumentException(
                "String value extends beyond end of data.", nameof(value));
        if (value.Length > this.Length)
            throw new ArgumentException("String is too long.");
        Array.Fill<byte>(data, 0x20, Offset, Length); // space-pad
        Encoding.ASCII.GetBytes(value, 0, value.Length, data, Offset);
    }
}

/// <summary>
/// Numeric (integer) parameter.
/// </summary>
public class NumericParameter : Parameter<int>
{
    public string Name { get; }
    public int Offset { get; }
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
    public NumericParameter(string name, int offset,
        int minValue = 0, int maxValue = 127,
        int bitCount = 7, int bitOffset = 0)
    {
        Debug.Assert(name != null);
        Debug.Assert(offset >= 0);
        Debug.Assert(maxValue >= minValue);
        Debug.Assert(bitCount > 0);
        Debug.Assert(bitOffset >= 0);

        Name = name;
        Offset = offset;
        MinValue = minValue;
        MaxValue = maxValue;
        BitCount = bitCount;
        BitOffset = bitOffset;
    }

    public override void Validate(int value)
    {
        if (value < MinValue)
            throw new ValidationException<int>(
                "Value is too small.", MinValue);
        if (value > MaxValue)
            throw new ValidationException<int>(
                "Value is too large.", MaxValue);
    }

    public override int GetValue(byte[] data)
    {
        if (Offset >= data.Length)
            throw new ArgumentException(
                "Data is not long enough");

        int mask = (1 << BitCount) - 1;
        int value = ((int)data[Offset] >> BitOffset) & mask;
        return value;
    }

    public override void SetValue(byte[] data, int value)
    {
        if (Offset >= data.Length)
            throw new ArgumentException(
                "Data is not long enough");

        int srcMask = (1 << BitCount) - 1;
        int dstMask = 0xff - (srcMask << BitOffset);
        value = (value << BitOffset) & srcMask;
        data[Offset] &= (byte)dstMask; // clear parameter's bit in byte
        data[Offset] |= (byte)value; // set parameter value
    }
}

