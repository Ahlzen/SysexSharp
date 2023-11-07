using System;
using System.Collections.Generic;
using System.Linq;
using Ahlzen.SysexSharp.SysexLib.Parsing;
using Ahlzen.SysexSharp.SysexLib.Utils;

namespace Ahlzen.SysexSharp.SysexLib.Manufacturers.Yamaha;

/// <summary>
/// Base class for DX/TX-series multi-item bank data dumps,
/// such as a DX7 32-voice bank.
/// </summary>
public abstract class DXBank : Sysex, IHasItems
{
    protected abstract byte?[] Header { get; }
    protected virtual int HeaderLength => Header.Length;
    protected virtual int BankDataLength => ItemCount * ItemSize;
    protected virtual int TotalLength => HeaderLength + BankDataLength + 2; // last 2 bytes are checksum + end-of-exclusive

    /// <summary>
    /// Parameters for parsing/updating each Item.
    /// </summary>
    /// <remarks>
    /// For example, for a DX7 32-voice bank, this has the parameters for the packed voice
    /// data format.
    /// Offsets are relative to the start of the item's data.
    /// </remarks>
    protected abstract List<Parameter> Parameters { get; }
    protected abstract Dictionary<string, Parameter> ParametersByName { get; }

    public abstract int ItemCount { get; }
    protected abstract int ItemSize { get; } // data size for each item, in bytes
    protected abstract string? ItemNameParameter { get; } // name of the parameter that parses the item name, e.g. "Voice name", if applicable


    internal DXBank(byte[] data, string? name) : base(data, name)
    {
        if (data.Length != TotalLength)
        {
            throw new ArgumentException(
                $"Data was not of the expected length. Expected: {TotalLength}, actual: {data.Length}.",
                nameof(data));
        }
    }

    public abstract override string? Device { get; }

    public abstract override string? Type { get; }

    #region IHasItems

    /// <see cref="IHasItems.GetItemNames"/>
    public virtual IEnumerable<string>? GetItemNames()
    {
        if (ItemNameParameter != null)
            for (int i = 0; i < ItemCount; i++)
                yield return (string) ParametersByName[ItemNameParameter].GetValue(_data, GetItemDataOffset(i));
    }

    /// <see cref="IHasItems.GetItem"/>
    public abstract Sysex GetItem(int index);

    /// <see cref="IHasItems.SetItem"/>
    public abstract void SetItem(int index, Sysex sysex);

    public Dictionary<string,object> ItemToDictionary(int index) {
        int offset = GetItemDataOffset(index);
        return Parameters.ToDictionary(p => p.Name, p => p.GetValue(_data, offset));
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Returns the parameter data offset for the specified item.
    /// </summary>
    /// <returns></returns>
    protected int GetItemDataOffset(int itemNumber)
        => HeaderLength + ItemSize * itemNumber;

    /// <summary>
    /// Gets the parameter data for the specified item.
    /// </summary>
    protected byte[] GetItemData(int itemNumber)
        => _data.SubArray(GetItemDataOffset(itemNumber), ItemSize);

    #endregion
}
