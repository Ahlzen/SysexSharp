using System;
using System.Collections.Generic;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;

internal static class YamahaFactory
{
    internal static Sysex Create(byte[] data, string? filename = null)
    {
        // Check known composite (multi) sysex types
        
        if (CompositeSysex.GetSysexCount(data) > 1)
        {
            var compositeSysex = new CompositeSysex(data);

            // TX81Z Voice = TX81Z Additional Voice Data + DX21 Voice
            if (compositeSysex.ItemCount == 2 &&
                compositeSysex.GetItem(0) is TX81ZAdditionalVoiceData &&
                compositeSysex.GetItem(1) is DX21Voice)
            {
                return new TX81ZVoice(data);
            }

            return new CompositeSysex(data);
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

        // TX81Z
        if (TX81ZVoiceBank.Test(data))
            return new TX81ZVoiceBank(data);
        if (TX81ZAdditionalVoiceData.Test(data))
            return new TX81ZAdditionalVoiceData(data);

        // Not a known type
        return new Sysex(data, filename);
    }
}
