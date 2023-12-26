using System.Collections.Generic;
using Ahlzen.SysexSharp.SysexLib.Parsing;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;

/// <summary>
/// Yamaha DX7 single-voice data dump.
/// </summary>
public class DX7Voice : DXVoice, ICanParse
{
    internal override byte?[] Header => DXData.DX7SingleVoiceHeader;

    protected override int ParameterDataLength => DXData.DX7ParameterDataLength;
    protected override List<Parameter> Parameters => DXData.DX7SingleVoiceParameters;
    protected override Dictionary<string, Parameter> ParametersByName => DXData.DX7SingleVoiceParametersByName;
    protected override string VoiceNameParameter => "Voice name";

    public override string? Device => "DX7";
    public override string? Type => "Single voice";

    public static bool Test(byte[] data)
    {
        if (!Sysex.Test(data)) return false;
        if (!ParsingUtils.MatchesPattern(data, DXData.DX7SingleVoiceHeader)) return false;
        if (data.Length != DXData.DX7SingleVoiceTotalLength) return false;
        return true;
    }

    internal DX7Voice(byte[] data) : base(data) { }

    internal DX7Voice(Dictionary<string, object> parameterValues)
        : base(FromParameterValues(DXData.DX7SingleVoiceParameters,
            parameterValues, DXData.DX7SingleVoiceTotalLength, DXData.DX7SingleVoiceHeader))
    { }
}
