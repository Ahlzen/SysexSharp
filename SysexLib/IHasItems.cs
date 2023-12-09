using System.Collections.Generic;

namespace Ahlzen.SysexSharp.SysexLib;

/// <summary>
/// Indicates that the Sysex contains individual (sub-)items,
/// such as patches that are part of a bank, that are
/// valid on their own and can be extracted into individual
/// Sysex objects.
/// </summary>
public interface IHasItems
{
    /// <summary>
    /// Returns the total number of items in this sysex.
    /// </summary>
    public int ItemCount { get; }

    /// <summary>
    /// Returns the names of the items in this sysex,
    /// or null if not applicable.
    /// </summary>
    public IEnumerable<string>? GetItemNames();

    /// <summary>
    /// Returns the specified item as a standalone sysex.
    /// </summary>
    public Sysex GetItem(int index);

    ///// <summary>
    ///// Updates the specified item in the current sysex. For example,
    ///// sets a particular patch in a bank (useful for assembling
    ///// custom patch banks). Supplied sysex must be the appropriate type.
    ///// </summary>
    //public void SetItem(int index, Sysex sysex);
}
