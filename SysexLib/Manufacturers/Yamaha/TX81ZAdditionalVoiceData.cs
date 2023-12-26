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
public class TX81ZAdditionalVoiceData : DXVoice, ICanParse
{
    internal override byte?[] Header => DXData.TX81ZAdditionalVoiceDataHeader;
    
    protected override int ParameterDataLength => 23; // does not include static part of heading
    protected override List<Parameter> Parameters => DXData.TX81ZAdditionalVoiceDataParameters;
    protected override Dictionary<string, Parameter> ParametersByName => DXData.TX81ZAdditionalVoiceDataParametersByName;
    protected override string? VoiceNameParameter => null;

    public override string? Device => "TX81Z";
    public override string? Type => "Additional voice data";

    public static bool Test(byte[] data)
    {
        if (!Sysex.Test(data)) return false;
        if (!ParsingUtils.MatchesPattern(data, DXData.TX81ZAdditionalVoiceDataHeader)) return false;
        if (data.Length != DXData.TX81ZAdditionalVoiceDataTotalLength) return false;
        return true;
    }

    internal TX81ZAdditionalVoiceData(byte[] data) : base(data) { }

    internal TX81ZAdditionalVoiceData(Dictionary<string, object> parameterValues)
        : base(FromParameterValues(DXData.TX81ZAdditionalVoiceDataParameters, parameterValues,
            DXData.TX81ZAdditionalVoiceDataTotalLength, DXData.TX81ZAdditionalVoiceDataHeader,
            DXData.TX81ZAdditionalVoiceDataChecksumDataStartOffset)) { }
}
