using Ahlzen.SysexSharp.SysexLib.Parsing;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;

/// <summary>
/// Single DX7 parameter change. May be either a voice
/// parameter or a global function.
/// </summary>
public class DX7ParameterChange : Sysex
{
    public DX7ParameterChange(byte[] data) :
        base(data, null, DX_TX_Data.DX7ParameterChangeLengthBytes)
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
    public int Group => GetGroup(_data);

    /// <see cref="Sysex.Test"/>
    public static bool Test(byte[] data)
    {
        if (!Sysex.Test(data)) return false;
        if (!ParsingUtils.MatchesPattern(data, DX_TX_Data.DX7ParameterChangeFormat)) return false;
        if (data.Length != DX_TX_Data.DX7ParameterChangeLengthBytes) return false;
        int group = GetGroup(data);
        if (!(group == 0 || group == 2)) return false;
        return true;
    }

    private static int GetGroup(byte[] data) => data[3] >> 2;
}
