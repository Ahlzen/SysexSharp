using Ahlzen.SysexSharp.SysexLib.Parsing;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;


/// <summary>
/// Single DX7 parameter change. May be either a voice
/// parameter or a global function.
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
    private const int ParameterChangeLengthBytes = 7;

    public DX7ParameterChange(byte[] data) :
        base(data, null, ParameterChangeLengthBytes)
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
        if (data.Length != ParameterChangeLengthBytes) return false;
        int group = GetGroup(data);
        if (!(group == 0 || group == 2)) return false;
        return true;
    }

    private static int GetGroup(byte[] data) => data[3] >> 2;
}


/// <summary>
/// Yamaha DX7 single-voice data dump.
/// </summary>
public class DX7Voice : Sysex
{
    private static readonly byte?[] SingleVoiceDataHeader = {
        0xf0, 0x43, 0x00, 0x00, 0x01, 0x1b };

    private const int SingleVoiceDataSize = (6 + 155 + 2);

    public DX7Voice(byte[] data, string? name = null)
        : base(data, name, SingleVoiceDataSize)
    {
    }

    public override string? Device => "DX7";

    public override string? Type => "Single voice";

    public new static bool Test(byte[] data)
    {
        if (!Sysex.Test(data)) return false;
        if (!ParsingUtils.MatchesPattern(data, SingleVoiceDataHeader)) return false;
        if (data.Length != SingleVoiceDataSize) return false;
        return true;
    }
}


/// <summary>
/// Yamaha DX7 32-voice bank data dump.
/// </summary>
public class DX7Bank : Sysex
{
    private static readonly byte?[] BankDataHeader = {
        0xf0, 0x43, 0x00, 0x09, 0x20, 0x00 };

    private const int PackedVoiceDataSize = 128;
    private const int BankDataSize = 6 + PackedVoiceDataSize * 32 + 2;

    public DX7Bank(byte[] data, string? name = null)
        : base(data, name, BankDataSize)
    {
    }

    public override string? Device => "DX7";

    public override string? Type => "32-voice bank";

    public new static bool Test(byte[] data)
    {
        if (!Sysex.Test(data)) return false;
        if (!ParsingUtils.MatchesPattern(data, BankDataHeader)) return false;
        if (data.Length != BankDataSize) return false;
        return true;
    }
}