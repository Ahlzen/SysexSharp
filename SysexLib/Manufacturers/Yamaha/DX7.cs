using Ahlzen.SysexSharp.SysexLib.Parsing;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;


/// <summary>
/// Sysex representing a change in a single DX7 parameter
/// value. Maybe either a voice parameter or a global function.
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
    public const int DX7ParameterChangeLengthBytes = 7;

    public DX7ParameterChange(byte[] data) :
        base(data, null, DX7ParameterChangeLengthBytes)
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
        if (data.Length != ParameterChangeFormat.Length) return false;
        int group = GetGroup(data);
        if (!(group == 0 || group == 2)) return false;
        return true;
    }

    private static int GetGroup(byte[] data) => data[3] >> 2;
}


public class DX7Voice //: Sysex
{
    // TODO
}

public class DX7Bank //: Sysex
{
    // TODO
}