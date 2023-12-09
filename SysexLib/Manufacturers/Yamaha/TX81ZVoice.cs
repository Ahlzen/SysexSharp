using System;
using System.Collections.Generic;
using Ahlzen.SysexSharp.SysexLib.Parsing;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha
{
    public class TX81ZVoice : CompositeSysex
    {
        public TX81ZVoice(byte[] data) : base(data)
        {
            if (ItemCount != 2)
                throw new ArgumentException("Data is not a TX81ZVoice: Should contain two sysex messages (DX21Voice + TX81Z Additional data).");
            if (GetItem(0) is not TX81ZAdditionalVoiceData)
                throw new ArgumentException("Data is not a TX81ZVoice: Second contained sysex should be TX81ZAdditionalData.");
            if (GetItem(1) is not DX21Voice)
                throw new ArgumentException("Data is not a TX81ZVoice: First contained sysex should be DX21Voice.");
        }

        public override string? Device => "TX81Z";
        public override string? Type => "Single voice";
        public override string? Name => GetItem(1).Name;

        public new static bool Test(byte[] data)
        {
            if (!Sysex.Test(data)) return false;

            // Should consist of TX81Z Additional Voice Data
            // + DX21 Voice Data
            List<int> offsets = GetSysexOffsets(data);
            if (offsets.Count != 2) return false;

            // 1st sysex: TX81Z Additional Voice Data
            if (!ParsingUtils.MatchesPattern(data,
               TX81ZAdditionalVoiceData.TX81ZAdditionalVoiceHeader, offsets[0])) return false;

            // 2nd sysex: DX21 Single Voice Data
            if (!ParsingUtils.MatchesPattern(data,
               DX21Voice.DX21SingleVoiceHeader, offsets[1])) return false;

            var compositeSysex = new CompositeSysex(data);
            if (compositeSysex.ItemCount != 2) return false;
            if (compositeSysex.GetItem(0) is not TX81ZAdditionalVoiceData) return false;
            if (compositeSysex.GetItem(1) is not DX21Voice) return false;

            return true;
        }
    }
}
