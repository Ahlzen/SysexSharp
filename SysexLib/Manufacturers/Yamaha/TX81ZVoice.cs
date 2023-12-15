﻿using System;
using System.Collections.Generic;
using Ahlzen.SysexSharp.SysexLib.Parsing;
using Ahlzen.SysexSharp.SysexLib.Utils;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha
{
    public class TX81ZVoice : CompositeSysex
    {
        /// <summary>
        /// These parameters are specific to DX21/DX27/DX100, and are
        /// not included in TX81Z data, so they need to be added
        /// (with blank values) during conversion.
        /// </summary>
        private static string[] DX21OnlyParameters = new[] {
            "Pitch EG Rate 1",
            "Pitch EG Rate 2",
            "Pitch EG Rate 3",
            "Pitch EG Level 1",
            "Pitch EG Level 2",
            "Pitch EG Level 3",
        };

        public TX81ZVoice(byte[] data) : base(data)
        {
            if (ItemCount != 2)
                throw new ArgumentException(
                    "Data is not a TX81ZVoice: Should contain two sysex messages (DX21Voice + TX81Z Additional data).");
            if (GetItem(0) is not TX81ZAdditionalVoiceData)
                throw new ArgumentException(
                    "Data is not a TX81ZVoice: Second contained sysex should be TX81ZAdditionalData.");
            if (GetItem(1) is not DX21Voice)
                throw new ArgumentException("Data is not a TX81ZVoice: First contained sysex should be DX21Voice.");
        }

        public TX81ZVoice(Dictionary<string, object> parameterValues)
            : this(FromParameterValues(parameterValues))
        { }

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

        public static byte[] FromParameterValues(
            Dictionary<string, object> parameterValues)
        {
            // Add DX21-specific parameters (not used, but must be supplied)
            var dx21ParameterValues = new Dictionary<string, object>(parameterValues);
            foreach (string dx21OnlyParameter in DX21OnlyParameters)
                dx21ParameterValues.Add(dx21OnlyParameter, 0);

            var tx81ZAdditionalData = new TX81ZAdditionalVoiceData(parameterValues);
            var dx21Voice = new DX21Voice(dx21ParameterValues);
            return tx81ZAdditionalData.GetData()
                .Append(dx21Voice.GetData());
        }
    }
}
