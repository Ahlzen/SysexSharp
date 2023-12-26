using System.Collections.Generic;
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
    internal override byte?[] Header => DXData.DX21SingleVoiceHeader;
    protected override int ParameterDataLength => 93;
    protected override List<Parameter> Parameters => DXData.DX21SingleVoiceParameters;
    protected override Dictionary<string, Parameter> ParametersByName => DXData.DX21SingleVoiceParametersByName;
    protected override string? VoiceNameParameter => "Voice name";

    public override string? Device => "DX21/DX27/DX100";
    public override string? Type => "Single voice";

    public new static bool Test(byte[] data)
    {
        if (!Sysex.Test(data)) return false;
        if (!ParsingUtils.MatchesPattern(data, DXData.DX21SingleVoiceHeader)) return false;
        if (data.Length != DXData.DX21SingleVoiceTotalLength) return false;
        return true;
    }

    internal DX21Voice(byte[] data) : base(data) { }

    internal DX21Voice(Dictionary<string, object> parameterValues)
        : base(FromParameterValues(DXData.DX21SingleVoiceParameters,
            parameterValues, DXData.DX21SingleVoiceTotalLength, DXData.DX21SingleVoiceHeader))
    { }
}
