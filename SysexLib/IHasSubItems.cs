using System.Collections.Generic;

namespace Ahlzen.SysexSharp.SysexLib;

/// <summary>
/// Indicates that the Sysex contains individual sub-items
/// (such as patches that are part of a bank) that are
/// valid on their own and can be extracted into individual
/// Sysex objects.
/// </summary>
public interface IHasSubItems
{
    /// <summary>
    /// Returns the total number of sub-items in this sysex.
    /// </summary>
    /// <returns></returns>
    public int GetSubItemCount();

    /// <summary>
    /// Returns the names of the sub-items in this sysex.
    /// Returns null if not applicable.
    /// </summary>
    public IEnumerable<string>? GetSubItemNames();

    /// <summary>
    /// Returns the specified sub-item as a standalone sysex.
    /// </summary>
    public Sysex GetSubItem(int index);

    /// <summary>
    /// Updates the specified sub-item in the current sysex. For example,
    /// sets a particular patch in a bank (useful for assembling
    /// custom patch banks)
    /// </summary>
    public void SetSubItem(int index, Sysex sysex);
}
