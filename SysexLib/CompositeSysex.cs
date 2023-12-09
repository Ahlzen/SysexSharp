using System;
using System.Collections.Generic;
using System.Linq;
using Ahlzen.SysexSharp.SysexLib.Utils;

namespace Ahlzen.SysexSharp.SysexLib;

/// <summary>
/// A sysex that may contain more than one Start/End-of-exclusive
/// markers (essentially multiple sysex messages combined).
/// </summary>
public class CompositeSysex : Sysex, IHasItems
{
    protected List<Sysex> Sysexes { get; } = new();

    public int ItemCount => Sysexes.Count;
    public Sysex GetItem(int index) => Sysexes[index];
    public IEnumerable<Sysex> GetItems() => Sysexes;
    public IEnumerable<string>? GetItemNames() => Sysexes.Select(s => s.Name ?? "");

    /// <summary>
    /// Create a new composite sysex from raw data containing at least
    /// one (but usually more) individual sysex messages.
    /// </summary>
    /// <remarks>
    /// Data would be e.g. [f0 sysex1data f7 f0 sysex2data f7 f0 sysex3data f7]
    /// </remarks>
    public CompositeSysex(byte[] data, string? filename = null) : base(data, filename)
    {
        int sysexIndex = 0;
        int start = -1;
        for (int i = 0; i < data.Length; i++)
        {
            if (start == -1)
            {
                start = i;
                if (data[i] != Constants.START_OF_SYSEX)
                    throw new ArgumentException(
                        $"Sysex number {sysexIndex + 1} does not have a valid start-of-sysex marker.");
            }
            if (data[i] == Constants.END_OF_SYSEX)
            {
                int end = i;
                byte[] sysexData = data.SubArray(start, end - start + 1);
                Sysex sysex = SysexFactory.Create(sysexData);
                Sysexes.Add(sysex);
                start = -1;
                sysexIndex++;
            }
        }
        // NOTE: base class constructor would already have checked
        // that the last byte is end-of-sysex
    }

    public static CompositeSysex FromSysexes(IEnumerable<Sysex> sysexes, string? filename)
    {
        var data = new List<byte>();
        foreach (Sysex sysex in sysexes)
            data.AddRange(sysex.GetData());
        return new CompositeSysex(data.ToArray(), filename);
    }

    /// <returns>
    /// Array of offsets (in bytes) of all start-of-sysex (0xF0) markers.
    /// </returns>
    public static List<int> GetSysexOffsets(byte[] data)
    {
        List<int> offsets = new List<int>(2);
        for (int i = 0; i < data.Length; i++)
            if (data[i] == Constants.START_OF_SYSEX)
                offsets.Add(i);
        return offsets;
    }

    /// <returns>
    /// Returns the number of start-of-sysex (0xF0) markers in data.
    /// </returns>
    public static int GetSysexCount(byte[] data)
    {
        int sysexCount = 0;
        for (int i = 0; i < data.Length; i++)
            if (data[i] == Constants.START_OF_SYSEX)
                sysexCount++;
        return sysexCount;
    }
}
