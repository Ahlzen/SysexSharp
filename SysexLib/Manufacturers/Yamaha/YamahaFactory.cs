namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;

internal static class YamahaFactory
{
    internal static Sysex Create(byte[] data, string? filename = null)
    {
        if (DX7Bank.Test(data))
            return new DX7Bank(data, filename);
        if (DX7Voice.Test(data))
            return new DX7Voice(data);
        if (DX7ParameterChange.Test(data))
            return new DX7ParameterChange(data);

        // Not a known type
        return new Sysex(data, filename);
    }
}
