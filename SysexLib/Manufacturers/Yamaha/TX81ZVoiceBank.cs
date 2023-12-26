using Ahlzen.SysexSharp.SysexLib.Parsing;
using System;
using System.Collections.Generic;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha
{
    /// <summary>
    /// TX81Z 32-Voice bank (VMEM).
    /// </summary>
    /// <remarks>
    /// This includes both VCED (DX21/27/100) parameters and the additional
    /// ACED (TX81Z-specific) parameter data.
    /// </remarks>
    public class TX81ZVoiceBank : DXBank
    {
        public override string? Device => "TX81Z";
        public override string? Type => "32-voice bank";

        public TX81ZVoiceBank(byte[] data, string? name = null) : base(data, name) { }

        // NOTE: TX81Z and DX21 appear to have the same header, so either
        // will pass. The YamahaFactory can try to determine which is which
        // by looking at some of the data that is unique to either.
        protected override byte?[] Header => DXData.DX21_TX81Z_BankHeader; //DXData.TX81ZBankHeader;

        protected override List<Parameter> Parameters => DXData.TX81ZPackedVoiceDataParameters;
        protected override Dictionary<string, Parameter> ParametersByName => DXData.TX81ZPackedVoiceDataParametersByName;

        public override int ItemCount => 32;
        protected override int ItemSize => DXData.TX81ZPackedVoiceDataSize;
        protected override string? ItemNameParameter => "Voice name";

        public static bool Test(byte[] data)
        {
            if (!Sysex.Test(data)) return false;
            if (data.Length != DXData.TX81ZBankSize) return false;
            
            if (!ParsingUtils.MatchesPattern(data, DXData.DX21_TX81Z_BankHeader))
                return false;

            // Check that unused bytes for each voice are set to expected values
            for (int voice = 0; voice < 32; voice++) {
                int voiceOffset = 6 + DXData.TX81ZPackedVoiceDataSize * voice;

                // The unused global pitch EG data seems to always be
                // set to [0x63, 0x63, 0x63, 0x32, 0x32, 0x32] in TX81Z dumps.
                if (!ParsingUtils.MatchesPattern(data,
                    DXData.TX81ZUnusedGlobalPitchEGData, voiceOffset + 67))
                        return false;

                if (!ParsingUtils.AreZero(data, voiceOffset + 84, 44)) return false;
            }

            return true;
        }

        public override Sysex GetItem(int index)
            => new TX81ZVoice(ItemToDictionary(index));

        public override void SetItem(int index, Sysex sysex)
        {
            // TODO: Implement!
            throw new NotImplementedException();
        }
    }
}
