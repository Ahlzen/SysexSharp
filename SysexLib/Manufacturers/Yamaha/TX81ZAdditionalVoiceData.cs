using Ahlzen.SysexSharp.SysexLib.Parsing;
using System.Collections.Generic;
using System.Linq;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;

/// <summary>
/// TX81Z Additional data for single voice (SCED) dump.
/// </summary>
/// <remarks>
/// This is typically recieved/transmitted together with a TX81ZVoice sysex.
/// </remarks>
public class TX81ZAdditionalVoiceData : DXVoice, ICanParse
{
    #region Static data

    internal static readonly byte?[] TX81ZAdditionalVoiceHeader = {
        0xf0, 0x43, null, 0x7e, 0x00, 0x21,
        // ASCII header "LM  8976AE"
        // NOTE: This part is considered part of the data, so it's included in the checksum.
        (byte)'L', (byte)'M', (byte)' ', (byte)' ', (byte)'8',
        (byte)'9', (byte)'7', (byte)'6', (byte)'A', (byte)'E'
    };

    private const int TotalLength_Const = 6 + 10 + 23 + 2;

    // Offsets are relative to the start of the Parameter Data section

    internal static readonly List<Parameter> TX81ZAdditionalVoiceParameters = new();
    internal static readonly Dictionary<string, Parameter> TX81ZAdditionalVoiceParametersByName;

    static TX81ZAdditionalVoiceData()
    {
        for (int op = 0; op < 4; op++)
        {
            var offset = (3 - op) * 5; // operators are stored in reverse order
            string prefix = "OP " + (op + 1) + " ";
            TX81ZAdditionalVoiceParameters.Add(new NumericParameter(offset + 0, prefix + "Fixed Frequency", 0, 1));
            TX81ZAdditionalVoiceParameters.Add(new NumericParameter(offset + 1, prefix + "Fixed Frequency Range", 0, 7));
            TX81ZAdditionalVoiceParameters.Add(new NumericParameter(offset + 2, prefix + "Frequency Range Fine", 0, 15));
            TX81ZAdditionalVoiceParameters.Add(new NumericParameter(offset + 3, prefix + "Operator Waveform", 0, 7));
            TX81ZAdditionalVoiceParameters.Add(new NumericParameter(offset + 4, prefix + "EG Shift", 0, 3));
        }
        TX81ZAdditionalVoiceParameters.Add(new NumericParameter(20, "Reverb Rate", 0, 7));
        TX81ZAdditionalVoiceParameters.Add(new NumericParameter(21, "Foot Controller Pitch", 0, 99));
        TX81ZAdditionalVoiceParameters.Add(new NumericParameter(22, "Foot Controller Amplitude", 0, 99));

        TX81ZAdditionalVoiceParametersByName = TX81ZAdditionalVoiceParameters.ToDictionary(p => p.Name);
    }

    #endregion

    protected override byte?[] Header => TX81ZAdditionalVoiceHeader;
    protected override int ChecksumDataStartOffset => 6; // ASCII header is included in checksum
    protected override int ParameterDataLength => 23;
    protected override List<Parameter> Parameters => TX81ZAdditionalVoiceParameters;
    protected override Dictionary<string, Parameter> ParametersByName => TX81ZAdditionalVoiceParametersByName;
    protected override string? VoiceNameParameter => null;

    public override string? Device => "TX81Z";
    public override string? Type => "Additional voice data";

    internal TX81ZAdditionalVoiceData(byte[] data) : base(data) { }

    internal TX81ZAdditionalVoiceData(Dictionary<string, object> parameterValues) :
        base(parameterValues, TotalLength_Const) { }
}
