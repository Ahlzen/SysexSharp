using System;
using System.Collections.Generic;
using Ahlzen.SysexSharp.SysexLib.Parsing;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;

/// <summary>
/// Yamaha DX7 32-voice bank data dump.
/// </summary>
public class DX7Bank : DXBank, IHasItems
{
    public override string? Device => "DX7";
    public override string? Type => "32-voice bank";

    public DX7Bank(byte[] data, string? name = null) : base(data, name) { }

    protected override byte?[] Header => DXData.DX7BankDataHeader;

    protected override List<Parameter> Parameters => DXData.DX7PackedVoiceParameters;
    protected override Dictionary<string, Parameter> ParametersByName => DXData.DX7PackedVoiceParametersByName;

    public override int ItemCount => 32;
    protected override int ItemSize => DXData.DX7PackedVoiceDataSize;
    protected override string? ItemNameParameter => "Voice name";

    public new static bool Test(byte[] data)
    {
        if (!Sysex.Test(data)) return false;
        if (!ParsingUtils.MatchesPattern(data, DXData.DX7BankDataHeader)) return false;
        if (data.Length != DXData.DX7BankDataSize) return false;
        return true;
    }

    public override Sysex GetItem(int index)
        => new DX7Voice(ItemToDictionary(index));

    public override void SetItem(int index, Sysex sysex)
    {
        // TODO: Implement!
        throw new NotImplementedException();
    }
}

