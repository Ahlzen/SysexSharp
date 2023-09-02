using Ahlzen.SysexSharp.SysexLib.Parsing;
using Ahlzen.SysexSharp.SysexLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;

/// <summary>
/// Base class for DX/TX-series single voice data dumps
/// (or single-voice related parseable sysex; in which case Name may
/// be null)
/// </summary>
public abstract class DXVoice : Sysex, ICanParse
{
    protected abstract byte?[] Header { get; }
    protected virtual int HeaderLength => Header.Length;
    protected abstract int ParameterDataLength { get; }
    protected virtual int TotalLength => HeaderLength + ParameterDataLength + 2; // last 2 bytes are checksum + end-of-exclusive
    protected abstract List<Parameter> Parameters { get; }
    protected abstract Dictionary<string, Parameter> ParametersByName { get; }
    protected abstract string? VoiceNameParameter { get; } // name of the parameter that parses the voice name, e.g. "Voice name", if applicable
    
    /// <summary>
    /// The offset at which the data used to calculate the checksum starts.
    /// Usually (default) right after the header. Sometimes, e.g. for some TX81Z sysexes,
    /// a fixed ASCII string before the parameter data is included in the checksum.
    /// </summary>
    protected virtual int ChecksumDataStartOffset => HeaderLength;

    /// <summary>
    /// Constructor from existing data.
    /// </summary>
    internal DXVoice(byte[] data) : base(data)
    {
        if (data.Length != TotalLength)
        {
            throw new ArgumentException(
                $"Data was not of the expected length. Expected: {TotalLength}, actual: {data.Length}.",
                nameof(data));
        }
    }

    /// <summary>
    /// Constructor from parameter values. Must include all names/values exactly.
    /// </summary>
    /// <param name="totalLength">
    /// Total number of bytes in sysex data.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown is a parameter value is missing.
    /// </exception>
    internal DXVoice(Dictionary<string,object> parameterValues, int totalLength) : base(totalLength)
    {
        Header.CopyNonNullTo(Data);
        foreach (Parameter parameter in Parameters)
        {
            if (!parameterValues.ContainsKey(parameter.Name))
                throw new ArgumentException($"Value for parameter \"{parameter.Name}\" not found", nameof(parameterValues));
            object parameterValue = parameterValues[parameter.Name];
            parameter.SetValue(Data, parameterValue, HeaderLength);
        }
        UpdateChecksum();
        Data[Data.Length - 1] = Constants.END_OF_SYSEX;
    }

    public abstract override string? Device { get; }

    public abstract override string? Type { get; }

    public override string? Name =>
        VoiceNameParameter == null ? null :
        ParametersByName[VoiceNameParameter].GetValue(Data, HeaderLength) as string;

    #region ICanParse

    public IEnumerable<string> ParameterNames =>
        Parameters.Select(p => p.Name);

    public object GetParameterValue(string parameterName) =>
        ParametersByName[parameterName].GetValue(Data, HeaderLength);

    public void Validate()
    {
        Parameters.ForEach(p => p.Validate(Data, HeaderLength));
    }

    public Dictionary<string, object> ToDictionary()
        => Parameters.ToDictionary(p => p.Name, p => p.GetValue(Data, HeaderLength));

    public string ToJSON() => JsonSerializer.Serialize(
        ToDictionary(), new JsonSerializerOptions { WriteIndented = true });

    internal void UpdateChecksum()
    {
        byte[] checksumData = Data.SubArray(ChecksumDataStartOffset, TotalLength-ChecksumDataStartOffset-2);
        byte checksum = Checksum.GetTwoComplement7Bit(checksumData);
        Data[TotalLength-2] = checksum;
    }

    #endregion
}
