using Ahlzen.SysexSharp.SysexLib.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;

/// <summary>
/// TX81Z One-voice data (VCED) dump.
/// </summary>
/// <remarks>
/// TODO: Does the DX100 use the same sysex format?
/// </remarks>
internal class TX81ZVoice : Sysex // ICanParse
{
    internal static readonly byte?[] SingleVoiceDataHeader =
    {
        0xf0, 0x43, null, 0x04, 0x10, 0x00
    };
    internal const int SingleVoiceDataHeaderLength = 6;
    internal const int SingleVoiceDataLength =
        SingleVoiceDataHeaderLength +
        93 + // data
        2; // checksum + end-of-sysex

    #region Parameters

    // Offsets are relative to the start of the Parameter Data section

    internal static readonly List<Parameter> Parameters = new();
    internal static readonly Dictionary<string, Parameter> ParametersByName;

    static TX81ZVoice()
    {
        // Common parameters
        Parameters.Add(new NumericParameter(52, "Algorithm", 0, 7));
        Parameters.Add(new NumericParameter(53, "Feedback", 0, 7));
        Parameters.Add(new NumericParameter(54, "LFO Speed", 0, 99));
        Parameters.Add(new NumericParameter(55, "LFO Delay", 0, 99));
        Parameters.Add(new NumericParameter(56, "Pitch Modulation Depth", 0, 99));
        Parameters.Add(new NumericParameter(57, "Amplitude Modulation Depth", 0, 99));
        Parameters.Add(new NumericParameter(58, "LFO Sync", 0, 1));
        Parameters.Add(new NumericParameter(59, "LFO Wave", 0, 3));
        Parameters.Add(new NumericParameter(60, "Pitch Modulation Sensitivity", 0, 7));
        Parameters.Add(new NumericParameter(61, "Amplitude Modulation Sensitivity", 0, 3));
        Parameters.Add(new NumericParameter(62, "Transpose", 0, 48)); // Center = 24
        Parameters.Add(new NumericParameter(63, "Poly/Mono", 0, 1));
        Parameters.Add(new NumericParameter(64, "Pitch Bend Range", 0, 12));
        Parameters.Add(new NumericParameter(65, "Portamento Mode", 0, 1));
        Parameters.Add(new NumericParameter(66, "Portamento Time", 0, 99));
        Parameters.Add(new NumericParameter(67, "Foot Control Volume", 0, 99));
        Parameters.Add(new NumericParameter(68, "Sustain", 0, 1));
        Parameters.Add(new NumericParameter(69, "Portamento", 0, 1));
        Parameters.Add(new NumericParameter(70, "Chorus", 0, 1));
        Parameters.Add(new NumericParameter(71, "Modulation Wheel Pitch", 0, 99));
        Parameters.Add(new NumericParameter(72, "Modulation Wheel Amplitude", 0, 99));
        Parameters.Add(new NumericParameter(73, "Breath Control Pitch", 0, 99));
        Parameters.Add(new NumericParameter(74, "Breath Control Amplitude", 0, 99));
        Parameters.Add(new NumericParameter(75, "Breath Control Pitch Bias", 0, 99)); // Center = 50
        Parameters.Add(new NumericParameter(76, "Breath Control EG Bias", 0, 99));
        Parameters.Add(new AsciiParameter(77, "Voice name", 10));
        // Parameter (bytes) 87-92 are not used
        Parameters.Add(new NumericParameter(93, "Operator 4-1 On/Off", 0, 15));

        // OP 1-4 parameters
        for (int op = 0; op < 4; op++)
        {
            var offset = (3 - op) * 13; // OPs are stored in reverse order
            string prefix = "OP " + (op + 1) + " ";
            Parameters.Add(new NumericParameter(offset + 0, prefix + "Attack Rate", 0, 31));
            Parameters.Add(new NumericParameter(offset + 1, prefix + "Decay 1 Rate", 0, 31));
            Parameters.Add(new NumericParameter(offset + 2, prefix + "Decay 2 Rate", 0, 31));
            Parameters.Add(new NumericParameter(offset + 3, prefix + "Release Rate", 1, 15));
            Parameters.Add(new NumericParameter(offset + 4, prefix + "Decay 1 Level", 0, 15));
            Parameters.Add(new NumericParameter(offset + 5, prefix + "Level Scaling", 0, 99));
            Parameters.Add(new NumericParameter(offset + 6, prefix + "Rate Scaling", 0, 3));
            Parameters.Add(new NumericParameter(offset + 7, prefix + "EG Bias Sensitivity", 0, 7));
            Parameters.Add(new NumericParameter(offset + 8, prefix + "Amplitude Modulation Enable", 0, 1));
            Parameters.Add(new NumericParameter(offset + 9, prefix + "Key Velocity Sensitivity", 0, 7));
            Parameters.Add(new NumericParameter(offset + 10, prefix + "Operator Output Level", 0, 99));
            Parameters.Add(new NumericParameter(offset + 11, prefix + "Frequency", 0, 63));
            Parameters.Add(new NumericParameter(offset + 12, prefix + "Detune", 0, 6)); // Center = 3
        }

        // Index by name
        ParametersByName = Parameters.ToDictionary(parameter => parameter.Name);
    }

    #endregion

    public TX81ZVoice(byte[] data)
        : base(data, null, SingleVoiceDataLength) { }

    public override string? Device => "TX81Z";

    public override string? Type => "Single voice";

    public override string? Name =>
        ParametersByName["Voice name"].GetValue(Data, SingleVoiceDataHeaderLength) as string;

    public new static bool Test(byte[] data)
    {
        if (data == null) return false;
        if (!Sysex.Test(data)) return false;
        if (!ParsingUtils.MatchesPattern(data, SingleVoiceDataHeader)) return false;
        if (data.Length != SingleVoiceDataLength) return false;
        return true;
    }
}
