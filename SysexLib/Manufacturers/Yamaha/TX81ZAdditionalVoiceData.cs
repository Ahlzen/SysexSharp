using Ahlzen.SysexSharp.SysexLib.Parsing;
using System.Collections.Generic;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;

/// <summary>
/// TX81Z Additional data for single voice dump (ACED).
/// </summary>
/// <remarks>
/// This is typically recieved/transmitted together with a
/// DX21 Voice (VCED) sysex.
/// </remarks>
public class TX81ZAdditionalVoiceData : DX_TX_Voice, ICanParse
{
    internal override byte?[] Header => DX_TX_Data.TX81ZAdditionalVoiceDataHeader;
    
    protected override int ParameterDataLength => 23; // does not include static part of heading
    protected override List<Parameter> Parameters => DX_TX_Data.TX81ZAdditionalVoiceDataParameters;
    protected override Dictionary<string, Parameter> ParametersByName => DX_TX_Data.TX81ZAdditionalVoiceDataParametersByName;
    protected override string? VoiceNameParameter => null;

    public override string? Device => "TX81Z";
    public override string? Type => "Additional voice data";

    public static bool Test(byte[] data)
    {
        if (!Sysex.Test(data)) return false;
        if (!ParsingUtils.MatchesPattern(data, DX_TX_Data.TX81ZAdditionalVoiceDataHeader)) return false;
        if (data.Length != DX_TX_Data.TX81ZAdditionalVoiceDataTotalLength) return false;
        return true;
    }

    internal TX81ZAdditionalVoiceData(byte[] data) : base(data) { }

    internal TX81ZAdditionalVoiceData(Dictionary<string, object> parameterValues)
        : base(FromParameterValues(DX_TX_Data.TX81ZAdditionalVoiceDataParameters, parameterValues,
            DX_TX_Data.TX81ZAdditionalVoiceDataTotalLength, DX_TX_Data.TX81ZAdditionalVoiceDataHeader,
            DX_TX_Data.TX81ZAdditionalVoiceDataChecksumDataStartOffset)) { }
}
