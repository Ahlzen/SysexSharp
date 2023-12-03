namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;

internal static class YamahaFactory
{
    internal static Sysex Create(byte[] data, string? filename = null)
    {
        // DX7
        if (DX7Bank.Test(data))
            return new DX7Bank(data, filename);
        if (DX7Voice.Test(data))
            return new DX7Voice(data);
        if (DX7ParameterChange.Test(data))
            return new DX7ParameterChange(data);

        // TX81Z
        if (TX81ZVoiceBank.Test(data))
            return new TX81ZVoiceBank(data);
        if (TX81ZVoice.Test(data))
            return new TX81ZVoice(data);
        if (TX81ZAdditionalVoiceData.Test(data))
            return new TX81ZAdditionalVoiceData(data);

        // Not a known type
        return new Sysex(data, filename);
    }
}
