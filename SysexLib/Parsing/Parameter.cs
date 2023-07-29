using System;

namespace Ahlzen.SysexSharp.SysexLib.Parsing;

public abstract class Parameter<T>
{
    /// <summary>
    /// Validates that the specified value is valid
    /// for this parameter (length, range, etc)
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown if the specified value is not within the allowable
    /// range.
    /// </exception>
    public abstract void Validate(T value);

    /// <summary>
    /// Returns this parameter's current value in the
    /// specified data. Does not validate the value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the value cannot be determined, for example
    /// if the specified data is invalid or too short.
    /// </exception>
    /// <param name="data">The full sysex data.</param>
    /// <returns></returns>
    public abstract T GetValue(byte[] data);

    /// <summary>
    /// Updates this parameter's current value in the
    /// specified data. Does not validate the value.
    /// </summary>
    /// <param name="data">The full sysex data.</param>
    public abstract void SetValue(byte[] data, T value);
}

/// <summary>
/// ASCII string parameter. Space (0x20) padded.
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
            throw new ArgumentNullException(nameof(value));
        if (value.Length > this.Length)
            throw new ArgumentException(
                "String is too long.", nameof(value));
        // TODO: Check for non-ascii and non-printable characters

    }

    public override string GetValue(byte[] data)
    {
        throw new NotImplementedException();
    }

    public override void SetValue(byte[] data, string value)
    {
        throw new NotImplementedException();
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
        Name = name;
        Offset = offset;
        MinValue = minValue;
        MaxValue = maxValue;
        BitCount = bitCount;
        BitOffset = bitOffset;
    }

    public override void Validate(int value)
    {
        throw new NotImplementedException();
    }

    public override int GetValue(byte[] data)
    {
        throw new NotImplementedException();
    }

    public override void SetValue(byte[] data, int value)
    {
        throw new NotImplementedException();
    }
}

