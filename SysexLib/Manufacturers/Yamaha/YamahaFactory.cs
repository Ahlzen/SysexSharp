namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;

internal static class YamahaFactory
{
    internal static Sysex Create(byte[] data, string? filename = null)
    {
        // Check known composite (multi) sysex types
        
        if (MultiPartSysex.GetSysexCount(data) > 1)
        {
            var multiPartSysex = new MultiPartSysex(data);

            // TX81Z Voice = TX81Z Additional Voice Data + DX21 Voice
            if (multiPartSysex.ItemCount == 2 &&
                multiPartSysex.GetSysex(0) is TX81ZAdditionalVoiceData &&
                multiPartSysex.GetSysex(1) is DX21Voice)
            {
                return new TX81ZVoice(data);
            }

            return new MultiPartSysex(data);
        }

        // DX7
        if (DX7Bank.Test(data))
            return new DX7Bank(data, filename);
        if (DX7Voice.Test(data))
            return new DX7Voice(data);
        if (DX7ParameterChange.Test(data))
            return new DX7ParameterChange(data);

        // DX21
        if (DX21Voice.Test(data))
            return new DX21Voice(data);
        if (DX21Bank.Test(data))
            return new DX21Bank(data, filename);

        // TX81Z
        if (TX81ZVoiceBank.Test(data))
            return new TX81ZVoiceBank(data, filename);
        if (TX81ZAdditionalVoiceData.Test(data))
            return new TX81ZAdditionalVoiceData(data);

        // Not a known type
        return new Sysex(data, filename);
    }
}
