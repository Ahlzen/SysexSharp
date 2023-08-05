using System.Text;

namespace Ahlzen.SysexSharp.SysexLib.Parsing;

/// <summary>
/// Static helpers for parsing and formatting.
/// </summary>
public static class ParsingUtils
{
    /// <summary>
    /// Returns true if there are any characters in s
    /// outside of the normal printable ASCII range.
    /// </summary>
    public static bool HasNonPrintableCharacters(string s)
    {
        foreach (char c in s)
            if (IsNonPrintable(c))
                return true;
        return false;
    }

    /// <summary>
    /// Replaces any characters in s outside of the normal
    /// printable ASCII range with replacement.
    /// </summary>
    /// <returns>The new string</returns>
    public static string ReplaceNonPrintableCharacters(string s, char replacement = ' ')
    {
        var sb = new StringBuilder(s);
        for (int i = 0; i < s.Length; i++)
            if (IsNonPrintable(sb[i]))
                sb[i] = replacement;
        return sb.ToString();
    }

    private static bool IsNonPrintable(char c)
        => (c < 0x20 || c > 126);



    // Static pattern matching functions

    /// <summary>
    /// Checks if the data matches (starts with) a specifications
    /// sequence of bytes (pattern).
    /// </summary>
    /// <param name="data">Actual values from sysex. </param>
    /// <param name="pattern"> Expected values. Null in pattern is wildcard
    /// (matches any byte in data).</param>
    /// <param name="offset">Offset (in data) where the pattern is expected to start.</param>
    /// <returns>True if data starts with the specified pattern.</returns>
    public static bool MatchesPattern(byte[] data, byte?[] pattern, int offset = 0)
    {
        if (data.Length < pattern.Length + offset) return false;
        for (int i = 0; i < pattern.Length; i++) {
            if (pattern[i] == null) continue; // null matches any value
            if (data[i+offset] != pattern[i]) return false;
        }
        return true;
    }
}
