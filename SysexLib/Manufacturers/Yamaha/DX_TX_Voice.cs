using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Ahlzen.SysexSharp.SysexLib.Parsing;
using Ahlzen.SysexSharp.SysexLib.Utils;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;

/// <summary>
/// Base class for DX/TX-series single voice data dumps
/// (or single-voice related parseable sysex; in which case Name may
/// be null)
/// </summary>
public abstract class DX_TX_Voice : Sysex, ICanParse
{
    internal abstract byte?[] Header { get; }
    internal virtual int HeaderLength => Header.Length;
    protected abstract int ParameterDataLength { get; }
    protected virtual int TotalLength => HeaderLength + ParameterDataLength + 2; // last 2 bytes are checksum + end-of-exclusive
    protected abstract List<Parameter> Parameters { get; }
    protected abstract Dictionary<string, Parameter> ParametersByName { get; }
    protected abstract string? VoiceNameParameter { get; } // name of the parameter that parses the voice name, e.g. "Voice name", if applicable
    

    /// <summary>
    /// Constructor from existing data.
    /// </summary>
    internal DX_TX_Voice(byte[] data) : base(data)
    {
        if (data.Length != TotalLength)
        {
            throw new ArgumentException(
                $"Data was not of the expected length. Expected: {TotalLength}, actual: {data.Length}.",
                nameof(data));
        }
    }

    /// <summary>
    /// Builds DXVoice data from parameter values.
    /// </summary>
    /// <param name="checksumDataOffset">
    /// Optional. Defaults to the length of the header, but may need to be specified
    /// explicitly. Sometimes (as with certain TX81Z sysex types, part of the header
    /// is also checksummed).
    /// </param>
    protected static byte[] FromParameterValues(
        IEnumerable<Parameter> parameters,
        Dictionary<string, object> parameterValues,
        int totalLength,
        byte?[] header,
        int? checksumDataOffset = null)
    {
        byte[] data = new byte[totalLength];
        header.CopyNonNullTo(data);
        foreach (Parameter parameter in parameters)
        {
            if (!parameterValues.ContainsKey(parameter.Name))
                throw new ArgumentException($"Value for parameter \"{parameter.Name}\" not found", nameof(parameterValues));
            object parameterValue = parameterValues[parameter.Name];
            parameter.SetValue(data, parameterValue, header.Length);
        }
        SetChecksum(data, checksumDataOffset ?? header.Length);
        data[^1] = Constants.END_OF_SYSEX;
        return data;
    }

    public abstract override string? Device { get; }

    public abstract override string? Type { get; }

    public override string? Name =>
        VoiceNameParameter == null ? null :
        ParametersByName[VoiceNameParameter].GetValue(_data, HeaderLength) as string;

    #region ICanParse

    public IEnumerable<string> ParameterNames
        => Parameters.Select(p => p.Name);

    public object GetParameterValue(string parameterName)
        => ParametersByName[parameterName].GetValue(_data, HeaderLength);

    public void Validate()
        => Parameters.ForEach(p => p.Validate(_data, HeaderLength));

    public Dictionary<string, object> ToDictionary()
        => Parameters.ToDictionary(p => p.Name, p => p.GetValue(_data, HeaderLength));

    public string ToJSON() => JsonSerializer.Serialize(
        ToDictionary(), new JsonSerializerOptions { WriteIndented = true });

    /// <summary>
    /// Updates the checksum byte in the specified sysex data.
    /// </summary>
    /// <param name="checksumDataOffset">
    /// Usually the length of the header, but sometimes (like with certain
    /// TX81Z sysex types, part of the header is also checksummed).
    /// </param>
    protected static void SetChecksum(byte[] data, int checksumDataOffset)
    {
        int checksumDataLength = data.Length - checksumDataOffset - 2;
        byte[] checksumData = data.SubArray(checksumDataOffset, checksumDataLength);
        byte checksum = Checksum.GetTwoComplement7Bit(checksumData);
        data[^2] = checksum; // checksum is second last byte
    }

    #endregion
}
