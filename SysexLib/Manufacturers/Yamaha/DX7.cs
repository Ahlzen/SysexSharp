using System.Collections.Generic;
using System.Linq;
using Ahlzen.SysexSharp.SysexLib.Parsing;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;


/// <summary>
/// Single DX7 parameter change. May be either a voice
/// parameter or a global function.
/// </summary>
public class DX7ParameterChange : Sysex
{
    private static readonly byte?[] ParameterChangeFormat = {
        0xf0, 0x43, 0x10, null, null, null, 0xf7
    };

    /// <summary>
    /// Parameter change messages are always 7 bytes in length:
    /// [0xf0, 0x43, 0x10, data, data, data, 0xf7]
    /// </summary>
    private const int ParameterChangeLengthBytes = 7;

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
public class DX7Voice : Sysex
{
    private static readonly byte?[] SingleVoiceDataHeader = {
        0xf0, 0x43, 0x00, 0x00, 0x01, 0x1b };

    private const int SingleVoiceDataSize = (6 + 155 + 2);

    #region Parameters

    protected static List<Parameter> Parameters;
    protected static Dictionary<string, Parameter> ParametersByName;

    static DX7Voice()
    {
        Parameters = new List<Parameter>();
        // OP 1-6 parameters
        for (int op = 0; op < 6; op++)
        {
            var offset = (5 - op) * 21; // OPs are stored in reverse order
            var prefix = "OP " + (op + 1) + " ";
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

        // Index by name
        ParametersByName = Parameters.ToDictionary(parameter => parameter.Name);
    }

    #endregion

    public DX7Voice(byte[] data, string? name = null)
        : base(data, name, SingleVoiceDataSize)
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
}


/// <summary>
/// Yamaha DX7 32-voice bank data dump.
/// </summary>
public class DX7Bank : Sysex
{
    private static readonly byte?[] BankDataHeader = {
        0xf0, 0x43, 0x00, 0x09, 0x20, 0x00 };

    private const int PackedVoiceDataSize = 128;
    private const int BankDataSize = 6 + PackedVoiceDataSize * 32 + 2;

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
}