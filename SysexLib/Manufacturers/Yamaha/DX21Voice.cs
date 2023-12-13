using System.Collections.Generic;
using System.Linq;
using Ahlzen.SysexSharp.SysexLib.Parsing;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;

/// <summary>
/// DX21/DX27/DX100 (and TX81Z, see remark) One-voice data dump.
/// </summary>
/// <remarks>
/// For the TX81Z, this is the VCED data, with additional parameters transmitted
/// in a separate sysex (TX81ZAdditionalVoiceData).
/// </remarks>
public class DX21Voice : DXVoice, ICanParse
{
    #region Static data

    internal static readonly byte?[] DX21SingleVoiceHeader = {
        0xf0, 0x43, null, 0x03, 0x00, 0x5d };

    private const int TotalLength_Const = 6 + 93 + 2; // need const for constructor

    // Offsets are relative to the start of the Parameter Data section

    internal static readonly List<Parameter> DX21SingleVoiceParameters = new();
    internal static readonly Dictionary<string, Parameter> DX21SingleVoiceParametersByName;

    static DX21Voice()
    {
        // Common parameters
        DX21SingleVoiceParameters.Add(new NumericParameter(52, "Algorithm", 0, 7));
        DX21SingleVoiceParameters.Add(new NumericParameter(53, "Feedback", 0, 7));
        DX21SingleVoiceParameters.Add(new NumericParameter(54, "LFO Speed", 0, 99));
        DX21SingleVoiceParameters.Add(new NumericParameter(55, "LFO Delay", 0, 99));
        DX21SingleVoiceParameters.Add(new NumericParameter(56, "Pitch Modulation Depth", 0, 99));
        DX21SingleVoiceParameters.Add(new NumericParameter(57, "Amplitude Modulation Depth", 0, 99));
        DX21SingleVoiceParameters.Add(new NumericParameter(58, "LFO Sync", 0, 1));
        DX21SingleVoiceParameters.Add(new NumericParameter(59, "LFO Wave", 0, 3));
        DX21SingleVoiceParameters.Add(new NumericParameter(60, "Pitch Modulation Sensitivity", 0, 7));
        DX21SingleVoiceParameters.Add(new NumericParameter(61, "Amplitude Modulation Sensitivity", 0, 3));
        DX21SingleVoiceParameters.Add(new NumericParameter(62, "Transpose", 0, 48)); // Center = 24
        DX21SingleVoiceParameters.Add(new NumericParameter(63, "Poly/Mono", 0, 1));
        DX21SingleVoiceParameters.Add(new NumericParameter(64, "Pitch Bend Range", 0, 12));
        DX21SingleVoiceParameters.Add(new NumericParameter(65, "Portamento Mode", 0, 1));
        DX21SingleVoiceParameters.Add(new NumericParameter(66, "Portamento Time", 0, 99));
        DX21SingleVoiceParameters.Add(new NumericParameter(67, "Foot Control Volume", 0, 99));
        DX21SingleVoiceParameters.Add(new NumericParameter(68, "Sustain", 0, 1));
        DX21SingleVoiceParameters.Add(new NumericParameter(69, "Portamento", 0, 1));
        DX21SingleVoiceParameters.Add(new NumericParameter(70, "Chorus", 0, 1));
        DX21SingleVoiceParameters.Add(new NumericParameter(71, "Modulation Wheel Pitch", 0, 99));
        DX21SingleVoiceParameters.Add(new NumericParameter(72, "Modulation Wheel Amplitude", 0, 99));
        DX21SingleVoiceParameters.Add(new NumericParameter(73, "Breath Control Pitch", 0, 99));
        DX21SingleVoiceParameters.Add(new NumericParameter(74, "Breath Control Amplitude", 0, 99));
        DX21SingleVoiceParameters.Add(new NumericParameter(75, "Breath Control Pitch Bias", 0, 99)); // Center = 50
        DX21SingleVoiceParameters.Add(new NumericParameter(76, "Breath Control EG Bias", 0, 99));
        DX21SingleVoiceParameters.Add(new AsciiParameter(77, "Voice name", 10));
        
        // Parameters (bytes) 87-92 are not used with TX81Z
        // TODO: Add parameters for DX21/DX27/DX100

        //DX21SingleVoiceParameters.Add(new NumericParameter(93, "Operator 4-1 On/Off", 0, 15));

        // OP 1-4 parameters
        for (int op = 0; op < 4; op++)
        {
            var offset = (3 - op) * 13; // OPs are stored in reverse order
            string prefix = "OP " + (op + 1) + " ";
            DX21SingleVoiceParameters.Add(new NumericParameter(offset + 0, prefix + "Attack Rate", 0, 31));
            DX21SingleVoiceParameters.Add(new NumericParameter(offset + 1, prefix + "Decay 1 Rate", 0, 31));
            DX21SingleVoiceParameters.Add(new NumericParameter(offset + 2, prefix + "Decay 2 Rate", 0, 31));
            DX21SingleVoiceParameters.Add(new NumericParameter(offset + 3, prefix + "Release Rate", 1, 15));
            DX21SingleVoiceParameters.Add(new NumericParameter(offset + 4, prefix + "Decay 1 Level", 0, 15));
            DX21SingleVoiceParameters.Add(new NumericParameter(offset + 5, prefix + "Level Scaling", 0, 99));
            DX21SingleVoiceParameters.Add(new NumericParameter(offset + 6, prefix + "Rate Scaling", 0, 3));
            DX21SingleVoiceParameters.Add(new NumericParameter(offset + 7, prefix + "EG Bias Sensitivity", 0, 7));
            DX21SingleVoiceParameters.Add(new NumericParameter(offset + 8, prefix + "Amplitude Modulation Enable", 0, 1));
            DX21SingleVoiceParameters.Add(new NumericParameter(offset + 9, prefix + "Key Velocity Sensitivity", 0, 7));
            DX21SingleVoiceParameters.Add(new NumericParameter(offset + 10, prefix + "Operator Output Level", 0, 99));
            DX21SingleVoiceParameters.Add(new NumericParameter(offset + 11, prefix + "Frequency", 0, 63));
            DX21SingleVoiceParameters.Add(new NumericParameter(offset + 12, prefix + "Detune", 0, 6)); // Center = 3
        }

        // Index by name
        DX21SingleVoiceParametersByName = DX21SingleVoiceParameters.ToDictionary(p => p.Name);
    }

    #endregion

    internal override byte?[] Header => DX21SingleVoiceHeader;
    protected override int ParameterDataLength => 93;
    protected override List<Parameter> Parameters => DX21SingleVoiceParameters;
    protected override Dictionary<string, Parameter> ParametersByName => DX21SingleVoiceParametersByName;
    protected override string? VoiceNameParameter => "Voice name";

    public override string? Device => "DX21/DX27/DX100";
    public override string? Type => "Single voice";

    public new static bool Test(byte[] data)
    {
        if (!Sysex.Test(data)) return false;
        if (!ParsingUtils.MatchesPattern(data, DX21SingleVoiceHeader)) return false;
        if (data.Length != TotalLength_Const) return false;
        return true;
    }

    internal DX21Voice(byte[] data) : base(data) { }

    internal DX21Voice(Dictionary<string, object> parameterValues)
        : base(FromParameterValues(DX21SingleVoiceParameters,
            parameterValues, TotalLength_Const, DX21SingleVoiceHeader))
    { }
}
