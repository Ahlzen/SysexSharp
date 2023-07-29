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
}
