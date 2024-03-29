﻿using System;
using System.Collections.Generic;
using Ahlzen.SysexSharp.SysexLib.Parsing;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha
{
    public class DX21Bank : DX_TX_Bank, IHasItems
    {
        public override string? Device => "DX21/DX27/DX100";
        public override string? Type => "32-voice bank";

        public DX21Bank(byte[] data, string? name = null) : base(data, name) { }

        protected override byte?[] Header => DX_TX_Data.DX21_TX81Z_BankHeader;

        protected override List<Parameter> Parameters => DX_TX_Data.DX21PackedVoiceDataParameters;
        protected override Dictionary<string, Parameter> ParametersByName => DX_TX_Data.DX21PackedVoiceDataParametersByName;

        public override int ItemCount => 32;
        protected override int ItemSize => DX_TX_Data.DX21PackedVoiceDataSize;
        protected override string? ItemNameParameter => "Voice name";

        public static bool Test(byte[] data)
        {
            if (!Sysex.Test(data)) return false;
            if (data.Length != DX_TX_Data.DX21BankSize) return false;

            if (!ParsingUtils.MatchesPattern(data, DX_TX_Data.DX21_TX81Z_BankHeader))
                return false;

            // Check that the trailing bytes for each voice (used e.g. for the
            // TX81Z) are zero
            for (int voice = 0; voice < 32; voice++) {
                int voiceOffset = 6 + DX_TX_Data.DX21PackedVoiceDataSize * voice;
                if (!ParsingUtils.AreZero(data, voiceOffset + 73, 55)) return false;
            }

            return true;
        }

        public override Sysex GetItem(int index)
            => new DX21Voice(ItemToDictionary(index));

        public override void SetItem(int index, Sysex sysex)
            => throw new NotImplementedException(); // TODO: implement
    }
}
