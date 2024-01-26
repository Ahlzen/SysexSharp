using System.Collections.Generic;
using System.Linq;
using Ahlzen.SysexSharp.SysexLib.Parsing;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha
{
    /// <summary>
    /// Static class with  data for DX/TX-series devices (header
    /// data, parameters, etc).
    /// </summary>
    /// <remarks>
    /// Many of these devices share parameters, hence the need
    /// for this class to avoid duplicated data.
    /// </remarks>
    internal static class DX_TX_Data
    {
        // DX7 parameter change

        internal static readonly byte?[] DX7ParameterChangeFormat = {
            0xf0, 0x43, 0x10, null, null, null, 0xf7 };
        /// <summary>
        /// Parameter change messages are always 7 bytes in length:
        /// [0xf0, 0x43, 0x10, data, data, data, 0xf7]
        /// </summary>
        internal const int DX7ParameterChangeLengthBytes = 7;


        // DX7 single-voice data

        internal static readonly byte?[] DX7SingleVoiceHeader = {
            0xf0, 0x43, 0x00, 0x00, 0x01, 0x1b };
        internal const int DX7ParameterDataLength = 155; // bytes
        internal const int DX7SingleVoiceTotalLength = 6 + DX7ParameterDataLength + 2; // needs const for constructor
        internal static readonly List<Parameter> DX7SingleVoiceParameters = new();
        internal static readonly Dictionary<string, Parameter> DX7SingleVoiceParametersByName;


        // DX7 32-voice bank data (packed)

        internal static readonly byte?[] DX7BankDataHeader = {
            0xf0, 0x43, 0x00, 0x09, 0x20, 0x00 };
        internal const int DX7PackedVoiceDataSize = 128;
        internal const int DX7BankDataSize =
            6 + DX7PackedVoiceDataSize * 32 + 2;
        internal static readonly List<Parameter> DX7PackedVoiceParameters = new();
        internal static readonly Dictionary<string, Parameter> DX7PackedVoiceParametersByName;


        // DX21/DX27/DX100 and TX81Z Non-packed (single-voice) data

        internal static readonly List<Parameter> DX21_TX81Z_CommonSingleVoiceParameters = new();
        
        internal static readonly byte?[] DX21SingleVoiceHeader = {
            0xf0, // start-of-exclusive
            0x43, // Yamaha
            null, // channel (0-15)
            0x03, // format # (1-voice bulk data)
            0x00, // data byte count (93) MSB
            0x5d, // data byte count (93) LSB
        };
        internal const int DX21SingleVoiceTotalLength = 6 + 93 + 2; // need const for constructor
        internal static readonly List<Parameter> DX21SingleVoiceParameters = new();
        internal static readonly List<Parameter> DX21OnlyVoiceParameters = new(); // parameters that do NOT apply to the TX81Z
        internal static readonly Dictionary<string, Parameter> DX21SingleVoiceParametersByName;

        //internal static readonly byte?[] TX81ZBasicVoiceHeader = {
        //    0xf0, // start-of-exclusive
        //    0x43, // Yamaha
        //    null, // channel (0-15)
        //    0x04, // format # (32-voice bulk data)
        //    0x10, // data byte count MSB
        //    0x00, // data byte count LSB
        //};
        //internal const int TX81ZBasicVoiceTotalLength = 6 + 93 + 2; // need const for constructor
        //internal static readonly List<Parameter> TX81ZBasicVoiceParameters = new();
        //internal static readonly Dictionary<string, Parameter> TX81ZBasicVoiceParametersByName;

        internal static readonly byte?[] TX81ZAdditionalVoiceDataHeader =
        {
            0xf0, 0x43, null, 0x7e, 0x00, 0x21,
            // ASCII header "LM  8976AE"
            // NOTE: This part is considered part of the data, so it's included in the checksum.
            (byte)'L', (byte)'M', (byte)' ', (byte)' ', (byte)'8',
            (byte)'9', (byte)'7', (byte)'6', (byte)'A', (byte)'E'
        };
        internal const int TX81ZAdditionalVoiceDataChecksumDataStartOffset = 6;
        internal const int TX81ZAdditionalVoiceDataTotalLength = 6+10+23+2; // TODO
        internal static readonly List<Parameter> TX81ZAdditionalVoiceDataParameters = new();
        internal static readonly Dictionary<string, Parameter> TX81ZAdditionalVoiceDataParametersByName;



        // DX21/27/100 and TX81Z Packed (32-voice) data

        internal static readonly List<Parameter> DX21_TX81Z_CommonPackedVoiceParameters = new();

        internal static readonly byte?[] DX21_TX81Z_BankHeader = {
            0xf0, // start-of-exclusive
            0x43, // Yamaha
            null, // channel (0-15)
            0x04, // format # (32-voice bulk data)
            0x20, // data byte count (4096) MSB
            0x00, // data byte count (4096) LSB
        };

        // DX21/27/100: Packed voice data bytes offset 73-127 (55 bytes) should be zero
        // TX81Z: Packed voice data bytes offsets 67-72 (6 bytes) and 84-127 (44 bytes) should be zero

        internal const int DX21PackedVoiceDataSize = 128; // bytes per voice (73 bytes data + 55 zero bytes)
        internal const int DX21BankSize = // 4104 bytes
            6 // header
            + 32 * DX21PackedVoiceDataSize // data bytes
            + 2; // checksum + end-of-exclusive
        internal static readonly List<Parameter> DX21PackedVoiceDataParameters = new();
        internal static readonly Dictionary<string, Parameter> DX21PackedVoiceDataParametersByName;

        // NOTE: This is the header format according to the manual, but it does not appear to be correct.
        // The header should be the same as the DX21BankHeader (as it is the same size). We had to look
        // in the data instead to determine which is which.
        //internal static readonly byte?[] TX81ZBankHeader = {
        //    0xf0, 0x43, null, 0x04, 0x10, 0x00
        //};

        internal const int TX81ZPackedVoiceDataSize = 128; // bytes per voice (67 bytes data + 6 zero bytes + 11 bytes data + 44 zero byte)
        internal const int TX81ZBankSize = 6 + 32 * TX81ZPackedVoiceDataSize + 2;
        internal static readonly List<Parameter> TX81ZPackedVoiceDataParameters = new();
        internal static readonly Dictionary<string, Parameter> TX81ZPackedVoiceDataParametersByName;
        internal static readonly byte[] TX81ZUnusedGlobalPitchEGData = {0x63, 0x63, 0x63, 0x32, 0x32, 0x32};


        static DX_TX_Data()
        {
            #region DX7 Single Voice Parameters

            // Common parameters
            DX7SingleVoiceParameters.AddRange(new Parameter[] {
                new NumericParameter(126, "Pitch EG Rate 1", 0, 99),
                new NumericParameter(127, "Pitch EG Rate 2", 0, 99),
                new NumericParameter(128, "Pitch EG Rate 3", 0, 99),
                new NumericParameter(129, "Pitch EG Rate 4", 0, 99),
                new NumericParameter(130, "Pitch EG Level 1", 0, 99),
                new NumericParameter(131, "Pitch EG Level 2", 0, 99),
                new NumericParameter(132, "Pitch EG Level 3", 0, 99),
                new NumericParameter(133, "Pitch EG Level 4", 0, 99),
                new NumericParameter(134, "Algorithm", 0, 31),
                new NumericParameter(135, "Feedback", 0, 7),
                new NumericParameter(136, "Oscillator sync", 0, 1),
                new NumericParameter(137, "LFO Speed", 0, 99),
                new NumericParameter(138, "LFO Delay", 0, 99),
                new NumericParameter(139, "LFO Pitch mod depth", 0, 99),
                new NumericParameter(140, "LFO Amp mod depth", 0, 99),
                new NumericParameter(141, "LFO Sync", 0, 1),
                new NumericParameter(142, "LFO Waveform", 0, 5),
                new NumericParameter(143, "Pitch mod sensitivity", 0, 7),
                new NumericParameter(144, "Transpose", 0, 48),
                new AsciiParameter(145, "Voice name", 10),
            });
            // OP 1-6 Parameters
            for (int op = 6; op >= 1; op--) {
                var offset = (6 - op) * 21; // OPs are stored in reverse order
                string prefix = "OP " + op + " ";
                DX7SingleVoiceParameters.AddRange(new Parameter[] {
                    new NumericParameter(offset + 0, prefix + "EG Rate 1", 0, 99),
                    new NumericParameter(offset + 1, prefix + "EG Rate 2", 0, 99),
                    new NumericParameter(offset + 2, prefix + "EG Rate 3", 0, 99),
                    new NumericParameter(offset + 3, prefix + "EG Rate 4", 0, 99),
                    new NumericParameter(offset + 4, prefix + "EG Level 1", 0, 99),
                    new NumericParameter(offset + 5, prefix + "EG Level 2", 0, 99),
                    new NumericParameter(offset + 6, prefix + "EG Level 3", 0, 99),
                    new NumericParameter(offset + 7, prefix + "EG Level 4", 0, 99),
                    new NumericParameter(offset + 8, prefix + "Keyboard level scale break point", 0, 99),
                    new NumericParameter(offset + 9, prefix + "Keyboard level scale left depth", 0, 99),
                    new NumericParameter(offset + 10, prefix + "Keyboard level scale right depth", 0, 99),
                    new NumericParameter(offset + 11, prefix + "Keyboard level scale left curve", 0, 3),
                    new NumericParameter(offset + 12, prefix + "Keyboard level scale right curve", 0, 3),
                    new NumericParameter(offset + 13, prefix + "Keyboard rate scaling", 0, 7),
                    new NumericParameter(offset + 14, prefix + "Amp mod sensitivity", 0, 3),
                    new NumericParameter(offset + 15, prefix + "Keyboard velocity sensitivity", 0, 7),
                    new NumericParameter(offset + 16, prefix + "Output level", 0, 99),
                    new NumericParameter(offset + 17, prefix + "Osc mode", 0, 1),
                    new NumericParameter(offset + 18, prefix + "Osc frequency coarse", 0, 31),
                    new NumericParameter(offset + 19, prefix + "Osc frequency fine", 0, 99),
                    new NumericParameter(offset + 20, prefix + "Osc detune", 0, 14),
                });
            }

            // Index by name
            DX7SingleVoiceParametersByName = DX7SingleVoiceParameters.ToDictionary(p => p.Name);

            #endregion

            #region DX7 Packed Voice Parameters

            // NOTE: Offsets are from the start of the parameter data block (not from start of Sysex)

            for (int op = 6; op >= 1; op--) {
                int offset = (6 - op) * 17; // OPs are stored in reverse order
                string prefix = "OP " + op + " ";
                DX7PackedVoiceParameters.AddRange(new Parameter[] {
                    new NumericParameter(offset + 0, prefix + "EG Rate 1", 0, 99),
                    new NumericParameter(offset + 1, prefix + "EG Rate 2", 0, 99),
                    new NumericParameter(offset + 2, prefix + "EG Rate 3", 0, 99),
                    new NumericParameter(offset + 3, prefix + "EG Rate 4", 0, 99),
                    new NumericParameter(offset + 4, prefix + "EG Level 1", 0, 99),
                    new NumericParameter(offset + 5, prefix + "EG Level 2", 0, 99),
                    new NumericParameter(offset + 6, prefix + "EG Level 3", 0, 99),
                    new NumericParameter(offset + 7, prefix + "EG Level 4", 0, 99),
                    new NumericParameter(offset + 8, prefix + "Keyboard level scale break point", 0, 99),
                    new NumericParameter(offset + 9, prefix + "Keyboard level scale left depth", 0, 99),
                    new NumericParameter(offset + 10, prefix + "Keyboard level scale right depth", 0, 99),
                    new NumericParameter(offset + 11, prefix + "Keyboard level scale left curve", 0, 3, 2, 2),
                    new NumericParameter(offset + 11, prefix + "Keyboard level scale right curve", 0, 3, 2, 0),
                    new NumericParameter(offset + 12, prefix + "Osc detune", 0, 14, 4, 3),
                    new NumericParameter(offset + 12, prefix + "Keyboard rate scaling", 0, 7, 3, 0),
                    new NumericParameter(offset + 13, prefix + "Keyboard velocity sensitivity", 0, 7, 3, 2),
                    new NumericParameter(offset + 13, prefix + "Amp mod sensitivity", 0, 3, 2, 0),
                    new NumericParameter(offset + 14, prefix + "Output level", 0, 99),
                    new NumericParameter(offset + 15, prefix + "Osc frequency coarse", 0, 31, 5, 1),
                    new NumericParameter(offset + 15, prefix + "Osc mode", 0, 1, 1, 0),
                    new NumericParameter(offset + 16, prefix + "Osc frequency fine", 0, 99),
                });
            }
            DX7PackedVoiceParameters.AddRange(new Parameter[] {
                new NumericParameter(102, "Pitch EG Rate 1", 0, 99),
                new NumericParameter(103, "Pitch EG Rate 2", 0, 99),
                new NumericParameter(104, "Pitch EG Rate 3", 0, 99),
                new NumericParameter(105, "Pitch EG Rate 4", 0, 99),
                new NumericParameter(106, "Pitch EG Level 1", 0, 99),
                new NumericParameter(107, "Pitch EG Level 2", 0, 99),
                new NumericParameter(108, "Pitch EG Level 3", 0, 99),
                new NumericParameter(109, "Pitch EG Level 4", 0, 99),
                new NumericParameter(110, "Algorithm", 0, 31, 5, 0),
                new NumericParameter(111, "Oscillator sync", 0, 1, 1, 3),
                new NumericParameter(111, "Feedback", 0, 7, 3, 0),
                new NumericParameter(112, "LFO Speed", 0, 99),
                new NumericParameter(113, "LFO Delay", 0, 99),
                new NumericParameter(114, "LFO Pitch mod depth", 0, 99),
                new NumericParameter(115, "LFO Amp mod depth", 0, 99),
                new NumericParameter(116, "Pitch mod sensitivity", 0, 7, 3, 4),
                new NumericParameter(116, "LFO Waveform", 0, 5, 3, 1),
                new NumericParameter(116, "LFO Sync", 0, 1, 1, 0),
                new NumericParameter(117, "Transpose", 0, 48),
                new AsciiParameter(118, "Voice name", 10),
            });
            DX7PackedVoiceParametersByName = DX7PackedVoiceParameters.ToDictionary(p => p.Name);

            #endregion

            #region DX21/27/100 and TX81Z Single Voice Parameters (VCED)

            // OP 1-4 parameters
            for (int op = 4; op >= 1; op--) {
                var offset = (4 - op) * 13; // OP data is stored in reverse order
                string prefix = "OP " + (op) + " ";
                DX21_TX81Z_CommonSingleVoiceParameters.AddRange(new[] {
                    new NumericParameter(offset + 0, prefix + "Attack Rate", 0, 31),
                    new NumericParameter(offset + 1, prefix + "Decay 1 Rate", 0, 31),
                    new NumericParameter(offset + 2, prefix + "Decay 2 Rate", 0, 31),
                    new NumericParameter(offset + 3, prefix + "Release Rate", 1, 15),
                    new NumericParameter(offset + 4, prefix + "Decay 1 Level", 0, 15),
                    new NumericParameter(offset + 5, prefix + "Level Scaling", 0, 99),
                    new NumericParameter(offset + 6, prefix + "Rate Scaling", 0, 3),
                    new NumericParameter(offset + 7, prefix + "EG Bias Sensitivity", 0, 7),
                    new NumericParameter(offset + 8, prefix + "Amplitude Modulation Enable", 0, 1),
                    new NumericParameter(offset + 9, prefix + "Key Velocity Sensitivity", 0, 7),
                    new NumericParameter(offset + 10, prefix + "Operator Output Level", 0, 99),
                    new NumericParameter(offset + 11, prefix + "Frequency", 0, 63),
                    new NumericParameter(offset + 12, prefix + "Detune", 0, 6), // Center = 3
                });
            }
            // Common parameters
            DX21_TX81Z_CommonSingleVoiceParameters.AddRange(new Parameter[] {
                new NumericParameter(52, "Algorithm", 0, 7),
                new NumericParameter(53, "Feedback", 0, 7),
                new NumericParameter(54, "LFO Speed", 0, 99),
                new NumericParameter(55, "LFO Delay", 0, 99),
                new NumericParameter(56, "Pitch Modulation Depth", 0, 99),
                new NumericParameter(57, "Amplitude Modulation Depth", 0, 99),
                new NumericParameter(58, "LFO Sync", 0, 1),
                new NumericParameter(59, "LFO Wave", 0, 3),
                new NumericParameter(60, "Pitch Modulation Sensitivity", 0, 7),
                new NumericParameter(61, "Amplitude Modulation Sensitivity", 0, 3),
                new NumericParameter(62, "Transpose", 0, 48), // Center = 24
                new NumericParameter(63, "Poly/Mono", 0, 1),
                new NumericParameter(64, "Pitch Bend Range", 0, 12),
                new NumericParameter(65, "Portamento Mode", 0, 1),
                new NumericParameter(66, "Portamento Time", 0, 99),
                new NumericParameter(67, "Foot Control Volume", 0, 99),
                new NumericParameter(68, "Sustain", 0, 1),
                new NumericParameter(69, "Portamento", 0, 1),
                new NumericParameter(70, "Chorus", 0, 1),
                new NumericParameter(71, "Modulation Wheel Pitch", 0, 99),
                new NumericParameter(72, "Modulation Wheel Amplitude", 0, 99),
                new NumericParameter(73, "Breath Control Pitch", 0, 99),
                new NumericParameter(74, "Breath Control Amplitude", 0, 99),
                new NumericParameter(75, "Breath Control Pitch Bias", 0, 99), // Center = 50
                new NumericParameter(76, "Breath Control EG Bias", 0, 99),
                new AsciiParameter(77, "Voice name", 10),
            });

            DX21OnlyVoiceParameters.AddRange(new Parameter[] {
                // Parameters (bytes) 87-92 are only used with DX21/DX27/DX100
                // (not used with TX81Z)
                new NumericParameter(87, "Pitch EG Rate 1", 0, 99),
                new NumericParameter(88, "Pitch EG Rate 2", 0, 99),
                new NumericParameter(89, "Pitch EG Rate 3", 0, 99),
                new NumericParameter(90, "Pitch EG Level 1", 0, 99),
                new NumericParameter(91, "Pitch EG Level 2", 0, 99),
                new NumericParameter(92, "Pitch EG Level 3", 0, 99),
            });

            DX21SingleVoiceParameters.AddRange(DX21_TX81Z_CommonSingleVoiceParameters);
            DX21SingleVoiceParameters.AddRange(DX21OnlyVoiceParameters);
            DX21SingleVoiceParametersByName = DX21SingleVoiceParameters.ToDictionary(p => p.Name);

            // The "basic" TX81Z Single voice data parameters are the common
            // ones. The TX81Z then adds additional voice data as a separate sysex.
            //TX81ZBasicVoiceParameters.AddRange(DX21_TX81Z_CommonSingleVoiceParameters);
            //TX81ZBasicVoiceParametersByName = TX81ZBasicVoiceParameters.ToDictionary(p => p.Name);

            // TX81Z Additional Voice Data Parameters
            for (int op = 4; op >= 1; op--) {
                var offset = (4 - op) * 5; // OPs are stored in reverse order
                string prefix = "OP " + op + " ";
                TX81ZAdditionalVoiceDataParameters.AddRange(new Parameter[]{
                    new NumericParameter(offset + 0, prefix + "Fixed Frequency", 0, 1),
                    new NumericParameter(offset + 1, prefix + "Fixed Frequency Range", 0, 7),
                    new NumericParameter(offset + 2, prefix + "Frequency Range Fine", 0, 15),
                    new NumericParameter(offset + 3, prefix + "Operator Waveform", 0, 7),
                    new NumericParameter(offset + 4, prefix + "EG Shift", 0, 3),
                });
            }
            TX81ZAdditionalVoiceDataParameters.AddRange(new Parameter[]{
                new NumericParameter(20, "Reverb Rate", 0, 7),
                new NumericParameter(21, "Foot Controller Pitch", 0, 99),
                new NumericParameter(22, "Foot Controller Amplitude", 0, 99),
            });
            TX81ZAdditionalVoiceDataParametersByName = TX81ZAdditionalVoiceDataParameters.ToDictionary(p => p.Name);

            #endregion

            #region DX21/DX27/DX100 and TX81Z Packed Voice (VMEM) Parameters

            // Common DX21/27/100 and TX81Z Packed Parameters
            for (int op = 4; op >= 1; op--) {
                var offset = (4 - op) * 10; // (OP data is stored in reverse order)
                string prefix = "OP " + op + " ";
                DX21_TX81Z_CommonPackedVoiceParameters.AddRange(new[]{
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
            }
            DX21_TX81Z_CommonPackedVoiceParameters.AddRange(new Parameter[] {
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
            });
            DX21PackedVoiceDataParameters.AddRange(DX21_TX81Z_CommonPackedVoiceParameters);
            TX81ZPackedVoiceDataParameters.AddRange(DX21_TX81Z_CommonPackedVoiceParameters);

            // 67-72 are DX21/27/100 only
            // On TX81Z these appear to always be [0x63, 0x63, 0x63, 0x32, 0x32, 0x32]
            DX21PackedVoiceDataParameters.AddRange(new Parameter[]{
                new NumericParameter(67, "Pitch EG Rate 1", 0, 99),
                new NumericParameter(68, "Pitch EG Rate 2", 0, 99),
                new NumericParameter(69, "Pitch EG Rate 3", 0, 99),
                new NumericParameter(70, "Pitch EG Level 1", 0, 99),
                new NumericParameter(71, "Pitch EG Level 2", 0, 99),
                new NumericParameter(72, "Pitch EG Level 3", 0, 99),
            });

            // 73-80 are TX81Z Extended OP data (not used on DX21/27/100)
            for (int op = 4; op >= 1; op--) {
                int offset = 73 + (3 - op) * 2; // OPs are stored in reverse order here too
                string prefix = "OP " + op + " ";
                TX81ZPackedVoiceDataParameters.AddRange(new[]{
                    new NumericParameter(offset+0, prefix+"EG Shift", 0, 3, 2, 4),
                    new NumericParameter(offset+0, prefix+"Fixed Frequency", 0, 1, 1, 3),
                    new NumericParameter(offset+0, prefix+"Fixed Frequency Range", 0, 7, 3, 1),
                    new NumericParameter(offset+1, prefix+"Operator Waveform", 0, 7, 3, 4),
                    new NumericParameter(offset+1, prefix+"Frequency Range Fine", 0, 15, 4, 0),
                });
            }
            // 81-83 are TX81Z additional common parameters (not used on DX21/27/100)
            TX81ZPackedVoiceDataParameters.AddRange(new Parameter[]{
                new NumericParameter(81, "Reverb Rate", 0, 7, 3, 0),
                new NumericParameter(82, "Foot Controller Pitch", 0, 99),
                new NumericParameter(83, "Foot Controller Amplitude", 0, 99),
            });

            // Index by name
            DX21PackedVoiceDataParametersByName = DX21PackedVoiceDataParameters.ToDictionary(p => p.Name);
            TX81ZPackedVoiceDataParametersByName = TX81ZPackedVoiceDataParameters.ToDictionary(p => p.Name);

            #endregion
        }
    }
}
