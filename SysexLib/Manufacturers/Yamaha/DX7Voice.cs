using System.Collections.Generic;
using Ahlzen.SysexSharp.SysexLib.Parsing;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;

/// <summary>
/// Yamaha DX7 single-voice data dump.
/// </summary>
public class DX7Voice : DX_TX_Voice, ICanParse
{
    internal override byte?[] Header => DX_TX_Data.DX7SingleVoiceHeader;

    protected override int ParameterDataLength => DX_TX_Data.DX7ParameterDataLength;
    protected override List<Parameter> Parameters => DX_TX_Data.DX7SingleVoiceParameters;
    protected override Dictionary<string, Parameter> ParametersByName => DX_TX_Data.DX7SingleVoiceParametersByName;
    protected override string VoiceNameParameter => "Voice name";

    public override string? Device => "DX7";
    public override string? Type => "Single voice";

    public static bool Test(byte[] data)
    {
        if (!Sysex.Test(data)) return false;
        if (!ParsingUtils.MatchesPattern(data, DX_TX_Data.DX7SingleVoiceHeader)) return false;
        if (data.Length != DX_TX_Data.DX7SingleVoiceTotalLength) return false;
        return true;
    }

    internal DX7Voice(byte[] data) : base(data) { }

    internal DX7Voice(Dictionary<string, object> parameterValues)
        : base(FromParameterValues(DX_TX_Data.DX7SingleVoiceParameters,
            parameterValues, DX_TX_Data.DX7SingleVoiceTotalLength, DX_TX_Data.DX7SingleVoiceHeader))
    { }
}
