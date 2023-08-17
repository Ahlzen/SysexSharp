using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Ahlzen.SysexSharp.SysexLib.Parsing;
using Ahlzen.SysexSharp.SysexLib.Utils;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;


/// <summary>
/// Single DX7 parameter change. May be either a voice
/// parameter or a global function.
/// </summary>
public class DX7ParameterChange : Sysex
{
    internal static readonly byte?[] ParameterChangeFormat = {
        0xf0, 0x43, 0x10, null, null, null, 0xf7
    };

    /// <summary>
    /// Parameter change messages are always 7 bytes in length:
    /// [0xf0, 0x43, 0x10, data, data, data, 0xf7]
    /// </summary>
    internal const int ParameterChangeLengthBytes = 7;

    public DX7ParameterChange(byte[] data) :
        base(data, null, ParameterChangeLengthBytes)
    {}

    public override string? Device => "DX7";

    public override string? Type => Group switch {
        0x00 => "Parameter change (voice)",
        0x02 => "Parameter change (function)",
        _ => null
    };

    /// <summary>
    /// Returns the value of the "group" parameter in the
    /// sysex. Should be either 0 (voice) or 2 (function).
    /// </summary>
    /// <returns></returns>
    public int Group => GetGroup(Data);

    /// <see cref="Sysex.Test"/>
    public new static bool Test(byte[] data)
    {
        if (!Sysex.Test(data)) return false;
        if (!ParsingUtils.MatchesPattern(data, ParameterChangeFormat)) return false;
        if (data.Length != ParameterChangeLengthBytes) return false;
        int group = GetGroup(data);
        if (!(group == 0 || group == 2)) return false;
        return true;
    }

    private static int GetGroup(byte[] data) => data[3] >> 2;
}


/// <summary>
/// Yamaha DX7 single-voice data dump.
/// </summary>
public class DX7Voice : Sysex, ICanParse
{
    internal static readonly byte[] SingleVoiceDataHeader = {
        0xf0, 0x43, 0x00, 0x00, 0x01, 0x1b };

    internal const int SingleVoiceDataSize = (6 + 155 + 2);

    #region Parameters

    internal static readonly List<Parameter> Parameters = new();
    internal static readonly Dictionary<string, Parameter> ParametersByName;

    static DX7Voice()
    {
        // Common parameters
        Parameters.Add(new NumericParameter(126, "Pitch EG Rate 1", 0, 99));
        Parameters.Add(new NumericParameter(127, "Pitch EG Rate 2", 0, 99));
        Parameters.Add(new NumericParameter(128, "Pitch EG Rate 3", 0, 99));
        Parameters.Add(new NumericParameter(129, "Pitch EG Rate 4", 0, 99));
        Parameters.Add(new NumericParameter(130, "Pitch EG Level 1", 0, 99));
        Parameters.Add(new NumericParameter(131, "Pitch EG Level 2", 0, 99));
        Parameters.Add(new NumericParameter(132, "Pitch EG Level 3", 0, 99));
        Parameters.Add(new NumericParameter(133, "Pitch EG Level 4", 0, 99));
        Parameters.Add(new NumericParameter(134, "Algorithm", 0, 31));
        Parameters.Add(new NumericParameter(135, "Feedback", 0, 7));
        Parameters.Add(new NumericParameter(136, "Oscillator sync", 0, 1));
        Parameters.Add(new NumericParameter(137, "LFO Speed", 0, 99));
        Parameters.Add(new NumericParameter(138, "LFO Delay", 0, 99));
        Parameters.Add(new NumericParameter(139, "LFO Pitch mod depth", 0, 99));
        Parameters.Add(new NumericParameter(140, "LFO Amp mod depth", 0, 99));
        Parameters.Add(new NumericParameter(141, "LFO Sync", 0, 1));
        Parameters.Add(new NumericParameter(142, "LFO Waveform", 0, 5));
        Parameters.Add(new NumericParameter(143, "Pitch mod sensitivity", 0, 7));
        Parameters.Add(new NumericParameter(144, "Transpose", 0, 48));
        Parameters.Add(new AsciiParameter(145, "Voice name", 10));

        // OP 1-6 parameters
        for (int op = 0; op < 6; op++)
        {
            var offset = (5 - op) * 21; // OPs are stored in reverse order
            string prefix = "OP " + (op + 1) + " ";
            Parameters.Add(new NumericParameter(offset + 0, prefix + "EG Rate 1", 0, 99));
            Parameters.Add(new NumericParameter(offset + 1, prefix + "EG Rate 2", 0, 99));
            Parameters.Add(new NumericParameter(offset + 2, prefix + "EG Rate 3", 0, 99));
            Parameters.Add(new NumericParameter(offset + 3, prefix + "EG Rate 4", 0, 99));
            Parameters.Add(new NumericParameter(offset + 4, prefix + "EG Level 1", 0, 99));
            Parameters.Add(new NumericParameter(offset + 5, prefix + "EG Level 2", 0, 99));
            Parameters.Add(new NumericParameter(offset + 6, prefix + "EG Level 3", 0, 99));
            Parameters.Add(new NumericParameter(offset + 7, prefix + "EG Level 4", 0, 99));
            Parameters.Add(new NumericParameter(offset + 8, prefix + "Keyboard level scale break point", 0, 99));
            Parameters.Add(new NumericParameter(offset + 9, prefix + "Keyboard level scale left depth", 0, 99));
            Parameters.Add(new NumericParameter(offset + 10, prefix + "Keyboard level scale right depth", 0, 99));
            Parameters.Add(new NumericParameter(offset + 11, prefix + "Keyboard level scale left curve", 0, 3));
            Parameters.Add(new NumericParameter(offset + 12, prefix + "Keyboard level scale right curve", 0, 3));
            Parameters.Add(new NumericParameter(offset + 13, prefix + "Keyboard rate scaling", 0, 7));
            Parameters.Add(new NumericParameter(offset + 14, prefix + "Amp mod sensitivity", 0, 3));
            Parameters.Add(new NumericParameter(offset + 15, prefix + "Keyboard velocity sensitivity", 0, 7));
            Parameters.Add(new NumericParameter(offset + 16, prefix + "Output level", 0, 99));
            Parameters.Add(new NumericParameter(offset + 17, prefix + "Osc mode", 0, 1));
            Parameters.Add(new NumericParameter(offset + 18, prefix + "Osc frequency coarse", 0, 31));
            Parameters.Add(new NumericParameter(offset + 19, prefix + "Osc frequency fine", 0, 99));
            Parameters.Add(new NumericParameter(offset + 20, prefix + "Osc detune", 0, 14));
        }

        // Index by name
        ParametersByName = Parameters.ToDictionary(parameter => parameter.Name);
    }

    #endregion

    public DX7Voice(byte[] data)
        : base(data, null, SingleVoiceDataSize)
    {
    }

    public override string? Device => "DX7";

    public override string? Type => "Single voice";

    public override string? Name =>
        ParametersByName["Voice name"].GetValue(Data) as string;

    public new static bool Test(byte[] data)
    {
        if (!Sysex.Test(data)) return false;
        if (!ParsingUtils.MatchesPattern(data, SingleVoiceDataHeader)) return false;
        if (data.Length != SingleVoiceDataSize) return false;
        return true;
    }

    #region ICanParse

    public IEnumerable<string> ParameterNames =>
        Parameters.Select(p => p.Name);

    public object GetParameterValue(string parameterName) =>
        ParametersByName[parameterName].GetValue(Data);

    public void Validate() {
        Parameters.ForEach(p => p.Validate(Data));
    }

    public Dictionary<string, object> ToDictionary()
        => Parameters.ToDictionary(p => p.Name, p => p.GetValue(Data));

    public string ToJSON() => JsonSerializer.Serialize(
        ToDictionary(), new JsonSerializerOptions { WriteIndented = true });

    #endregion
}

/// <summary>
/// Wraps raw packed voice data for a single voice
/// (such as in a 32-voice bank sysex)
/// </summary>
internal class DX7PackedVoiceData
{
    public int Offset { get; set; }
    public byte[] Data { get; set; }

    internal const int PackedVoiceDataSize = 128;

    #region Parameters

    internal static readonly List<Parameter> Parameters = new();
    internal static readonly Dictionary<string, Parameter> ParametersByName;
    
    static DX7PackedVoiceData()
    {
        // Packed single-voice layout (used in 32-voice bulk dumps)
        // NOTE: Offsets are from the start of the parameter data block (not from start of Sysex)

        // Common parameters
        Parameters.Add(new NumericParameter(102, "Pitch EG Rate 1", 0, 99));
        Parameters.Add(new NumericParameter(103, "Pitch EG Rate 2", 0, 99));
        Parameters.Add(new NumericParameter(104, "Pitch EG Rate 3", 0, 99));
        Parameters.Add(new NumericParameter(105, "Pitch EG Rate 4", 0, 99));
        Parameters.Add(new NumericParameter(106, "Pitch EG Level 1", 0, 99));
        Parameters.Add(new NumericParameter(107, "Pitch EG Level 2", 0, 99));
        Parameters.Add(new NumericParameter(108, "Pitch EG Level 3", 0, 99));
        Parameters.Add(new NumericParameter(109, "Pitch EG Level 4", 0, 99));
        Parameters.Add(new NumericParameter(110, "Algorithm", 0, 31, 5, 0));
        Parameters.Add(new NumericParameter(111, "Oscillator sync", 0, 1, 1, 3));
        Parameters.Add(new NumericParameter(111, "Feedback", 0, 7, 3, 0));
        Parameters.Add(new NumericParameter(112, "LFO Speed", 0, 99));
        Parameters.Add(new NumericParameter(113, "LFO Delay", 0, 99));
        Parameters.Add(new NumericParameter(114, "LFO Pitch mod depth", 0, 99));
        Parameters.Add(new NumericParameter(115, "LFO Amp mod depth", 0, 99));
        Parameters.Add(new NumericParameter(116, "Pitch mod sensitivity", 0, 7, 3, 4));
        Parameters.Add(new NumericParameter(116, "LFO Waveform", 0, 5, 3, 1));
        Parameters.Add(new NumericParameter(116, "LFO Sync", 0, 1, 1, 0));
        Parameters.Add(new NumericParameter(117, "Transpose", 0, 48));
        Parameters.Add(new AsciiParameter(118, "Voice name", 10));

        // OP 1-6 parameters
        for (int op = 0; op < 6; op++)
        {
            int offset = (5 - op) * 17; // OPs are stored in reverse order
            string prefix = "OP " + (op + 1) + " ";
            Parameters.Add(new NumericParameter(offset + 0, prefix + "EG Rate 1", 0, 99));
            Parameters.Add(new NumericParameter(offset + 1, prefix + "EG Rate 2", 0, 99));
            Parameters.Add(new NumericParameter(offset + 2, prefix + "EG Rate 3", 0, 99));
            Parameters.Add(new NumericParameter(offset + 3, prefix + "EG Rate 4", 0, 99));
            Parameters.Add(new NumericParameter(offset + 4, prefix + "EG Level 1", 0, 99));
            Parameters.Add(new NumericParameter(offset + 5, prefix + "EG Level 2", 0, 99));
            Parameters.Add(new NumericParameter(offset + 6, prefix + "EG Level 3", 0, 99));
            Parameters.Add(new NumericParameter(offset + 7, prefix + "EG Level 4", 0, 99));
            Parameters.Add(new NumericParameter(offset + 8, prefix + "Keyboard level scale break point", 0, 99));
            Parameters.Add(new NumericParameter(offset + 9, prefix + "Keyboard level scale left depth", 0, 99));
            Parameters.Add(new NumericParameter(offset + 10, prefix + "Keyboard level scale right depth", 0, 99));
            Parameters.Add(new NumericParameter(offset + 11, prefix + "Keyboard level scale left curve", 0, 3, 2, 2));
            Parameters.Add(new NumericParameter(offset + 11, prefix + "Keyboard level scale right curve", 0, 3, 2, 0));
            Parameters.Add(new NumericParameter(offset + 12, prefix + "Osc detune", 0, 14, 4, 3));
            Parameters.Add(new NumericParameter(offset + 12, prefix + "Keyboard rate scaling", 0, 7, 3, 0));
            Parameters.Add(new NumericParameter(offset + 13, prefix + "Keyboard velocity sensitivity", 0, 7, 3, 2));
            Parameters.Add(new NumericParameter(offset + 13, prefix + "Amp mod sensitivity", 0, 3, 2, 0));
            Parameters.Add(new NumericParameter(offset + 14, prefix + "Output level", 0, 99));
            Parameters.Add(new NumericParameter(offset + 15, prefix + "Osc frequency coarse", 0, 31, 5, 1));
            Parameters.Add(new NumericParameter(offset + 15, prefix + "Osc mode", 0, 1, 1, 0));
            Parameters.Add(new NumericParameter(offset + 16, prefix + "Osc frequency fine", 0, 99));
        }

        // Index by name
        ParametersByName = Parameters.ToDictionary(parameter => parameter.Name);
    }

    #endregion

    /// <param name="data">Raw data.</param>
    /// <param name="offset">Optional. Offset (in data) to the start of this packed voice.</param>
    public DX7PackedVoiceData(byte[] data, int offset = 0)
    {
        Offset = offset;
        Data = data;

        if (data.Length < offset + PackedVoiceDataSize - 1)
            throw new ArgumentException("Data is not long enough.");
    }

    public Dictionary<string, object> ToDictionary(string keyPrefix = "")
        => Parameters.ToDictionary(p => keyPrefix + p.Name, p => p.GetValue(Data, Offset));

    public DX7Voice ToSingleVoice()
    {
        // Initialize raw data
        byte[] singleVoiceData = new byte[DX7Voice.SingleVoiceDataSize];
        Array.Copy(DX7Voice.SingleVoiceDataHeader, singleVoiceData, DX7Voice.SingleVoiceDataHeader.Length);
        singleVoiceData[singleVoiceData.Length - 1] = Constants.END_OF_SYSEX;

        // Transcode values
        Dictionary<string, object> parsedValues = ToDictionary();
        DX7Voice.Parameters.ForEach(parameter =>
            parameter.SetValue(singleVoiceData, parsedValues[parameter.Name]));

        return new DX7Voice(singleVoiceData);
    }
}

/// <summary>
/// Yamaha DX7 32-voice bank data dump.
/// </summary>
public class DX7Bank : Sysex, ICanParse, IHasSubItems
{
    internal static readonly byte[] BankDataHeader = {
        0xf0, 0x43, 0x00, 0x09, 0x20, 0x00 };

    
    internal const int BankDataSize =
        6 + DX7PackedVoiceData.PackedVoiceDataSize * 32 + 2;

    public DX7Bank(byte[] data, string? name = null)
        : base(data, name, BankDataSize)
    {
    }

    public override string? Device => "DX7";

    public override string? Type => "32-voice bank";

    public new static bool Test(byte[] data)
    {
        if (!Sysex.Test(data)) return false;
        if (!ParsingUtils.MatchesPattern(data, BankDataHeader)) return false;
        if (data.Length != BankDataSize) return false;
        return true;
    }

    /// <summary>
    /// Returns the packed voice data (128 bytes) for a single voice in the bank.
    /// </summary>
    /// <param name="voiceNumber">The specified voice number (0-31)</param>
    private byte[] GetPackedVoiceData(int voiceNumber)
        => Data.SubArray(GetVoiceDataOffset(voiceNumber), 128);

    private int GetVoiceDataOffset(int voiceNumber)
        => 6 + 32 * voiceNumber;

    #region ICanParse

    public IEnumerable<string> ParameterNames => throw new NotImplementedException();

    public object GetParameterValue(string parameterName)
    {
        throw new NotImplementedException();
    }

    public void Validate()
    {
        throw new NotImplementedException();
    }

    public Dictionary<string, object> ToDictionary()
    {
        throw new NotImplementedException();
    }

    public string ToJSON()
    {
        throw new NotImplementedException();
    }

    #endregion

    #region IHasSubItems

    public int GetSubItemCount() => 32;

    public IEnumerable<string>? GetSubItemNames()
    {
        Parameter voiceNameParameter =
            DX7PackedVoiceData.ParametersByName["Voice name"];
        for (int voice = 0; voice < 32; voice++)
            yield return voiceNameParameter.GetValue(Data, GetVoiceDataOffset(voice)) as string;
    }

    public Sysex GetSubItem(int index)
    {
        byte[] packedVoiceData = GetPackedVoiceData(index);
        var packedVoice = new DX7PackedVoiceData(packedVoiceData);
        return packedVoice.ToSingleVoice();
    }

    public void SetSubItem(int index, Sysex sysex)
    {
        throw new NotImplementedException();
    }

    #endregion
}