using System;
using System.Collections.Generic;
using System.Linq;
using Ahlzen.SysexSharp.SysexLib.Parsing;
using Ahlzen.SysexSharp.SysexLib.Utils;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;

/// <summary>
/// Yamaha DX7 32-voice bank data dump.
/// </summary>
public class DX7Bank : DXBank, IHasItems
{
    internal static readonly byte?[] BankDataHeader = {
        0xf0, 0x43, 0x00, 0x09, 0x20, 0x00 };

    internal const int PackedVoiceDataSize = 128;

    internal const int BankDataSize =
        6 + PackedVoiceDataSize * 32 + 2;

    #region Parameters

    internal static readonly List<Parameter> DX7PackedVoiceParameters = new();
    internal static readonly Dictionary<string, Parameter> DX7PackedVoiceParametersByName;

    static DX7Bank()
    {
        // Packed single-voice layout (used in 32-voice bulk dumps)
        // NOTE: Offsets are from the start of the parameter data block (not from start of Sysex)

        // Common parameters
        DX7PackedVoiceParameters.Add(new NumericParameter(102, "Pitch EG Rate 1", 0, 99));
        DX7PackedVoiceParameters.Add(new NumericParameter(103, "Pitch EG Rate 2", 0, 99));
        DX7PackedVoiceParameters.Add(new NumericParameter(104, "Pitch EG Rate 3", 0, 99));
        DX7PackedVoiceParameters.Add(new NumericParameter(105, "Pitch EG Rate 4", 0, 99));
        DX7PackedVoiceParameters.Add(new NumericParameter(106, "Pitch EG Level 1", 0, 99));
        DX7PackedVoiceParameters.Add(new NumericParameter(107, "Pitch EG Level 2", 0, 99));
        DX7PackedVoiceParameters.Add(new NumericParameter(108, "Pitch EG Level 3", 0, 99));
        DX7PackedVoiceParameters.Add(new NumericParameter(109, "Pitch EG Level 4", 0, 99));
        DX7PackedVoiceParameters.Add(new NumericParameter(110, "Algorithm", 0, 31, 5, 0));
        DX7PackedVoiceParameters.Add(new NumericParameter(111, "Oscillator sync", 0, 1, 1, 3));
        DX7PackedVoiceParameters.Add(new NumericParameter(111, "Feedback", 0, 7, 3, 0));
        DX7PackedVoiceParameters.Add(new NumericParameter(112, "LFO Speed", 0, 99));
        DX7PackedVoiceParameters.Add(new NumericParameter(113, "LFO Delay", 0, 99));
        DX7PackedVoiceParameters.Add(new NumericParameter(114, "LFO Pitch mod depth", 0, 99));
        DX7PackedVoiceParameters.Add(new NumericParameter(115, "LFO Amp mod depth", 0, 99));
        DX7PackedVoiceParameters.Add(new NumericParameter(116, "Pitch mod sensitivity", 0, 7, 3, 4));
        DX7PackedVoiceParameters.Add(new NumericParameter(116, "LFO Waveform", 0, 5, 3, 1));
        DX7PackedVoiceParameters.Add(new NumericParameter(116, "LFO Sync", 0, 1, 1, 0));
        DX7PackedVoiceParameters.Add(new NumericParameter(117, "Transpose", 0, 48));
        DX7PackedVoiceParameters.Add(new AsciiParameter(118, "Voice name", 10));

        // OP 1-6 parameters
        for (int op = 0; op < 6; op++)
        {
            int offset = (5 - op) * 17; // OPs are stored in reverse order
            string prefix = "OP " + (op + 1) + " ";
            DX7PackedVoiceParameters.Add(new NumericParameter(offset + 0, prefix + "EG Rate 1", 0, 99));
            DX7PackedVoiceParameters.Add(new NumericParameter(offset + 1, prefix + "EG Rate 2", 0, 99));
            DX7PackedVoiceParameters.Add(new NumericParameter(offset + 2, prefix + "EG Rate 3", 0, 99));
            DX7PackedVoiceParameters.Add(new NumericParameter(offset + 3, prefix + "EG Rate 4", 0, 99));
            DX7PackedVoiceParameters.Add(new NumericParameter(offset + 4, prefix + "EG Level 1", 0, 99));
            DX7PackedVoiceParameters.Add(new NumericParameter(offset + 5, prefix + "EG Level 2", 0, 99));
            DX7PackedVoiceParameters.Add(new NumericParameter(offset + 6, prefix + "EG Level 3", 0, 99));
            DX7PackedVoiceParameters.Add(new NumericParameter(offset + 7, prefix + "EG Level 4", 0, 99));
            DX7PackedVoiceParameters.Add(new NumericParameter(offset + 8, prefix + "Keyboard level scale break point", 0, 99));
            DX7PackedVoiceParameters.Add(new NumericParameter(offset + 9, prefix + "Keyboard level scale left depth", 0, 99));
            DX7PackedVoiceParameters.Add(new NumericParameter(offset + 10, prefix + "Keyboard level scale right depth", 0, 99));
            DX7PackedVoiceParameters.Add(new NumericParameter(offset + 11, prefix + "Keyboard level scale left curve", 0, 3, 2, 2));
            DX7PackedVoiceParameters.Add(new NumericParameter(offset + 11, prefix + "Keyboard level scale right curve", 0, 3, 2, 0));
            DX7PackedVoiceParameters.Add(new NumericParameter(offset + 12, prefix + "Osc detune", 0, 14, 4, 3));
            DX7PackedVoiceParameters.Add(new NumericParameter(offset + 12, prefix + "Keyboard rate scaling", 0, 7, 3, 0));
            DX7PackedVoiceParameters.Add(new NumericParameter(offset + 13, prefix + "Keyboard velocity sensitivity", 0, 7, 3, 2));
            DX7PackedVoiceParameters.Add(new NumericParameter(offset + 13, prefix + "Amp mod sensitivity", 0, 3, 2, 0));
            DX7PackedVoiceParameters.Add(new NumericParameter(offset + 14, prefix + "Output level", 0, 99));
            DX7PackedVoiceParameters.Add(new NumericParameter(offset + 15, prefix + "Osc frequency coarse", 0, 31, 5, 1));
            DX7PackedVoiceParameters.Add(new NumericParameter(offset + 15, prefix + "Osc mode", 0, 1, 1, 0));
            DX7PackedVoiceParameters.Add(new NumericParameter(offset + 16, prefix + "Osc frequency fine", 0, 99));
        }

        // Index by name
        DX7PackedVoiceParametersByName = DX7PackedVoiceParameters.ToDictionary(parameter => parameter.Name);
    }

    #endregion

    public override string? Device => "DX7";
    public override string? Type => "32-voice bank";

    public DX7Bank(byte[] data, string? name = null) : base(data, name) { }

    protected override byte?[] Header => BankDataHeader;

    protected override List<Parameter> Parameters => DX7PackedVoiceParameters;
    protected override Dictionary<string, Parameter> ParametersByName => DX7PackedVoiceParametersByName;

    public override int ItemCount => 32;
    protected override int ItemSize => PackedVoiceDataSize;
    protected override string? ItemNameParameter => "Voice name";

    public new static bool Test(byte[] data)
    {
        if (!Sysex.Test(data)) return false;
        if (!ParsingUtils.MatchesPattern(data, BankDataHeader)) return false;
        if (data.Length != BankDataSize) return false;
        return true;
    }

    public override Sysex GetItem(int index)
        => new DX7Voice(ItemToDictionary(index));

    public override void SetItem(int index, Sysex sysex)
    {
        // TODO: Implement!
        throw new NotImplementedException();
    }
}

/*
 
Original version:

public class DX7Bank : Sysex, IHasSubItems
{
    internal static readonly byte[] BankDataHeader = {
        0xf0, 0x43, 0x00, 0x09, 0x20, 0x00 };

    internal const int PackedVoiceDataSize = 128;

    internal const int BankDataSize =
        6 + PackedVoiceDataSize * 32 + 2;

    #region Parameters

    internal static readonly List<Parameter> Parameters = new();
    internal static readonly Dictionary<string, Parameter> ParametersByName;

    static DX7Bank()
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
        => 6 + 128 * voiceNumber;

    private Dictionary<string,object> VoiceToDictionary(int voiceNumber)
    {
        int offset = GetVoiceDataOffset(voiceNumber);
        return Parameters.ToDictionary(p => p.Name, p => p.GetValue(Data, offset));
    }

    #region IHasSubItems

    public int SubItemCount => 32;

    public IEnumerable<string>? GetSubItemNames()
    {
        Parameter voiceNameParameter = ParametersByName["Voice name"];
        for (int voice = 0; voice < 32; voice++)
            yield return voiceNameParameter.GetValue(Data, GetVoiceDataOffset(voice)) as string ?? "";
    }

    /// <returns>
    /// DX7Voice sysex for the specified voice number (0-31)
    /// </returns>
    public Sysex GetSubItem(int voiceNumber)
        => new DX7Voice(VoiceToDictionary(voiceNumber));

    public void SetSubItem(int index, Sysex sysex)
    {
        // TODO: Implement
        throw new NotImplementedException();
    }

    #endregion
}

 * */