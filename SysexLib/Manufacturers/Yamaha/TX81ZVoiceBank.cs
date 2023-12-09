using Ahlzen.SysexSharp.SysexLib.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha
{
    /// <summary>
    /// TX81Z 32-Voice bank (VMEM).
    /// </summary>
    /// <remarks>
    /// This includes both VCED (DX21/27/100) parameters and the additional
    /// ACED (TX81Z-specific) parameter data.
    /// </remarks>
    internal class TX81ZVoiceBank : DXBank
    {
        // This is the correct header according to the TX81Z manual
        internal static readonly byte?[] BankDataHeader = {
            0xf0, 0x43, null, 0x04, 0x10, 0x00 };

        // This is what I seem to come across in the wild...
        internal static readonly byte?[] BankDataHeader_Alt = {
            0xf0, 0x43, null, 0x04, 0x20, 0x00 };

        internal const int PackedVoiceDataSize = 128;

        internal const int BankDataSize = 6 + 32 * PackedVoiceDataSize + 2; // 4104 bytes

        #region Parameters

        internal static readonly List<Parameter> TX81ZPackedVoiceParameters = new();
        internal static readonly Dictionary<string, Parameter> TX81ZPackedVoiceParametersByName;

        static TX81ZVoiceBank()
        {
            // OP 1-4 parameters
            for (int op = 0; op < 4; op++)
            {
                string prefix = "OP " + (op + 1) + " ";
                // Standard OP data 
                var offset = (3 - op) * 10; // OPs are stored in reverse order
                TX81ZPackedVoiceParameters.AddRange(new[]{
                    new NumericParameter(offset+0, prefix+"Attack Rate", 0, 31, 5, 0),
                    new NumericParameter(offset+1, prefix+"Decay 1 Rate", 0, 31, 5, 0),
                    new NumericParameter(offset+2, prefix+"Decay 2 Rate", 0, 31, 5, 0),
                    new NumericParameter(offset+3, prefix+"Release Rate", 0, 15, 4, 0),
                    new NumericParameter(offset+4, prefix+"Decay 1 Level", 0, 15, 4, 0),
                    new NumericParameter(offset+5, prefix+"Level Scaling", 0, 99, 7, 0),
                    new NumericParameter(offset+6, prefix+"Amplitude Modulation Enable", 0, 1, 1, 6),
                    new NumericParameter(offset+6, prefix+"EG Bias Sensitivity", 0, 7, 3, 3),
                    new NumericParameter(offset+6, prefix+"Key Velocity Sensitivity", 0, 7, 3, 0),
                    new NumericParameter(offset+7, prefix+"Operator Output Level", 0, 99, 7, 0),
                    new NumericParameter(offset+8, prefix+"Frequency", 0, 63, 6, 0),
                    new NumericParameter(offset+9, prefix+"Rate Scaling", 0, 3, 2, 3),
                    new NumericParameter(offset+9, prefix+"Detune", 0, 6, 3, 0),
                });
                // Extended TX81Z OP data
                offset = 73 + (3 - op) * 2; // OPs are stored in reverse order here too
                TX81ZPackedVoiceParameters.AddRange(new[]{
                    new NumericParameter(offset+0, prefix+"EG Shift", 0, 3, 2, 4),
                    new NumericParameter(offset+0, prefix+"Fixed Frequency", 0, 1, 1, 3),
                    new NumericParameter(offset+0, prefix+"Fixed Frequency Range", 0, 7, 3, 1),
                    new NumericParameter(offset+1, prefix+"Operator Waveform", 0, 7, 3, 4),
                    new NumericParameter(offset+1, prefix+"Frequency Range Fine", 0, 15, 4, 0),
                });
            }
            // Common parameters
            TX81ZPackedVoiceParameters.AddRange(new Parameter[]{
                new NumericParameter(40, "LFO Sync", 0, 1, 1, 6),
                new NumericParameter(40, "Feedback", 0, 7, 3, 3),
                new NumericParameter(40, "Algorithm", 0, 7, 3, 0),
                new NumericParameter(41, "LFO Speed", 0, 99),
                new NumericParameter(42, "LFO Delay", 0, 99),
                new NumericParameter(43, "Pitch Modulation Depth", 0, 99),
                new NumericParameter(44, "Amplitude Modulation Depth", 0, 99),
                new NumericParameter(45, "Pitch Modulation Sensitivity", 0, 7, 3, 4),
                new NumericParameter(45, "Amplitude Modulation Sensitivity", 0, 3, 2, 2),
                new NumericParameter(45, "LFO Wave", 0, 3, 2, 0),
                new NumericParameter(46, "Transpose", 0, 48, 6, 0),
                new NumericParameter(47, "Pitch Bend Range", 0, 12, 4, 0),
                new NumericParameter(48, "Chorus", 0, 1, 1, 4),
                new NumericParameter(48, "Poly/Mono", 0, 1, 1, 3),
                new NumericParameter(48, "Sustain", 0, 1, 1, 2),
                new NumericParameter(48, "Portamento", 0, 1, 1, 1),
                new NumericParameter(48, "Portamento Mode", 0, 1, 1, 0),
                new NumericParameter(49, "Portamento Time", 0, 99),
                new NumericParameter(50, "Foot Control Volume", 0, 99),
                new NumericParameter(51, "Modulation Wheel Pitch", 0, 99),
                new NumericParameter(52, "Modulation Wheel Amplitude", 0, 99),
                new NumericParameter(53, "Breath Control Pitch", 0, 99),
                new NumericParameter(54, "Breath Control Amplitude", 0, 99),
                new NumericParameter(55, "Breath Control Pitch Bias", 0, 99),
                new NumericParameter(56, "Breath Control EG Bias", 0, 99),
                new AsciiParameter(57, "Voice name", 10),
                // 67-72 not used on TX81Z (DX21 etc only)
                // 73-80 are TX81Z additional OP data (see above)
                // 81-83 are TX81Z additional common parameters
                new NumericParameter(81, "Reverb Rate", 0, 7, 3, 0),
                new NumericParameter(82, "Foot Controller Pitch", 0, 99),
                new NumericParameter(83, "Foot Controller Amplitude", 0, 99),
            });

            // Index by name
            TX81ZPackedVoiceParametersByName = TX81ZPackedVoiceParameters.ToDictionary(p => p.Name);
        }

        #endregion

        public override string? Device => "TX81Z";
        public override string? Type => "32-voice bank";

        public TX81ZVoiceBank(byte[] data, string? name = null) : base(data, name) { }

        protected override byte?[] Header => BankDataHeader;

        protected override List<Parameter> Parameters => TX81ZPackedVoiceParameters;
        protected override Dictionary<string, Parameter> ParametersByName => TX81ZPackedVoiceParametersByName;

        public override int ItemCount => 32;
        protected override int ItemSize => PackedVoiceDataSize;
        protected override string? ItemNameParameter => "Voice name";

        public new static bool Test(byte[] data)
        {
            if (!Sysex.Test(data)) return false;
            if (!(ParsingUtils.MatchesPattern(data, BankDataHeader) ||
                ParsingUtils.MatchesPattern(data, BankDataHeader_Alt)))
                return false;
            if (data.Length != BankDataSize) return false;
            return true;
        }

        public override Sysex GetItem(int index)
            => new DX21Voice(ItemToDictionary(index)); // TODO: Change TX81ZVoice to incude DX21+Extended data!

        public override void SetItem(int index, Sysex sysex)
        {
            // TODO: Implement!
            throw new NotImplementedException();
        }
    }
}
