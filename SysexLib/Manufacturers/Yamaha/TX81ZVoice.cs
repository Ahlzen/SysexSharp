using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Ahlzen.SysexSharp.SysexLib.Parsing;
using Ahlzen.SysexSharp.SysexLib.Utils;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha
{
    public class TX81ZVoice : MultiPartSysex, ICanParse
    {
        public TX81ZVoice(byte[] data) : base(data)
        {
            if (ItemCount != 2)
                throw new ArgumentException(
                    "Data is not a TX81ZVoice: Should contain two sysex messages (DX21Voice + TX81Z Additional data).");
            if (GetSysex(0) is not TX81ZAdditionalVoiceData)
                throw new ArgumentException(
                    "Data is not a TX81ZVoice: Second contained sysex should be TX81ZAdditionalData.");
            if (GetSysex(1) is not DX21Voice)
                throw new ArgumentException("Data is not a TX81ZVoice: First contained sysex should be DX21Voice.");
        }

        public TX81ZVoice(Dictionary<string, object> parameterValues)
            : this(FromParameterValues(parameterValues))
        { }

        public override string? Device => "TX81Z";
        public override string? Type => "Single voice";
        public override string? Name => GetSysex(1).Name;

        private const int DX21VoiceOffset = DX_TX_Data.TX81ZAdditionalVoiceDataTotalLength;
        private const int TX81ZAdditionalDataOffset = 0;

        public new static bool Test(byte[] data)
        {
            if (!Sysex.Test(data)) return false;

            // Should consist of TX81Z Additional Voice Data
            // + DX21 Voice Data
            List<int> offsets = GetSysexOffsets(data);
            if (offsets.Count != 2) return false;

            // 1st sysex: TX81Z Additional Voice Data
            if (!ParsingUtils.MatchesPattern(data,
               DX_TX_Data.TX81ZAdditionalVoiceDataHeader, offsets[0])) return false;

            // 2nd sysex: DX21 Single Voice Data
            if (!ParsingUtils.MatchesPattern(data,
               DX_TX_Data.DX21SingleVoiceHeader, offsets[1])) return false;

            var multiPartSysex = new MultiPartSysex(data);
            if (multiPartSysex.ItemCount != 2) return false;
            if (multiPartSysex.GetSysex(0) is not TX81ZAdditionalVoiceData) return false;
            if (multiPartSysex.GetSysex(1) is not DX21Voice) return false;

            return true;
        }

        public static byte[] FromParameterValues(
            Dictionary<string, object> parameterValues)
        {
            // Add DX21-specific parameters (not used, but must be supplied)
            var dx21ParameterValues = new Dictionary<string, object>(parameterValues);
            foreach (Parameter dx21OnlyParameter in DX_TX_Data.DX21OnlyVoiceParameters)
                dx21ParameterValues.Add(dx21OnlyParameter.Name, 0);

            var tx81ZAdditionalData = new TX81ZAdditionalVoiceData(parameterValues);
            var dx21Voice = new DX21Voice(dx21ParameterValues);
            return tx81ZAdditionalData.GetData()
                .Append(dx21Voice.GetData());
        }

        #region ICanParse

        public IEnumerable<string> ParameterNames =>
            DX_TX_Data.DX21SingleVoiceParametersByName.Keys
            .Union(DX_TX_Data.TX81ZAdditionalVoiceDataParametersByName.Keys);

        public object GetParameterValue(string parameterName)
        {
            if (DX_TX_Data.DX21SingleVoiceParametersByName.ContainsKey(parameterName))
                return DX_TX_Data.DX21SingleVoiceParametersByName[parameterName].GetValue(
                    _data, DX21VoiceOffset);
            if (DX_TX_Data.TX81ZAdditionalVoiceDataParametersByName.ContainsKey(parameterName))
                return DX_TX_Data.TX81ZAdditionalVoiceDataParametersByName[parameterName].GetValue(
                    _data, TX81ZAdditionalDataOffset);
            throw new ArgumentException($"Parameter \"{parameterName}\" not found", nameof(parameterName));
        }

        public void Validate()
        {
            DX_TX_Data.DX21SingleVoiceParameters.ForEach(
                p => p.Validate(_data, DX21VoiceOffset));
            DX_TX_Data.TX81ZAdditionalVoiceDataParameters.ForEach(
                p => p.Validate(_data, TX81ZAdditionalDataOffset));
        }

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> dict = DX_TX_Data.DX21SingleVoiceParameters.ToDictionary(
                p => p.Name, p => p.GetValue(_data, DX21VoiceOffset));
            DX_TX_Data.TX81ZAdditionalVoiceDataParameters.ForEach(p =>
                dict.Add(p.Name, p.GetValue(_data, TX81ZAdditionalDataOffset)));
            return dict;
        }

        public string ToJSON() => JsonSerializer.Serialize(
            ToDictionary(), new JsonSerializerOptions { WriteIndented = true });

        #endregion
    }
}
