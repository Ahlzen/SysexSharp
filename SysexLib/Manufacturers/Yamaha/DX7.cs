namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;

public class DX7ParameterChange : Sysex
{
    /// <summary>
    /// Parameter change messages are always 7 bytes in length:
    /// [0xf0, 0x43, 0x10, data, data, data, 0xf7]
    /// </summary>
    public const int DX7ParameterChangeLengthBytes = 7;

    public DX7ParameterChange(byte[] rawData) :
        base(rawData, null, DX7ParameterChangeLengthBytes)
    {
    }

    public override string? Device => "DX7";
    public override string? Type => "Parameter change";
}

public class DX7Voice //: Sysex
{
    // TODO
}

public class DX7Bank //: Sysex
{
    // TODO
}