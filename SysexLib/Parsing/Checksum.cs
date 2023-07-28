namespace Ahlzen.SysexSharp.SysexLib.Parsing;

/// <summary>
/// Helpers for computing checksum values using ocmmon algorithms.
/// </summary>
internal static class Checksum
{
    /// <summary>
    /// Computes 2-complement checksum (lower 7 bits) of
    /// the specified data.
    /// Used e.g.by Yamaha.
    /// </summary>
    /// <param name="data">Data for which to compute checksum.</param>
    public static byte GetTwoComplement7Bit(byte[] data)
    {
        int sum = 0;
        for (int i = 0; i < data.Length; sum += data[i++]);
        sum &= 0x7f;
        int checksum = (128 - sum) & 0xf7;
        return (byte)checksum;
    }

    /// <summary>
    /// Computes the xor (7 bits) of the specified data.
    /// Used e.g. as checksum in SDS messages.
    /// </summary>
    /// <param name="data">Data for which to compute checksum.</param>
    /// <param name="offset">Bytes prior to this are ignored. (optional)</param>
    /// <param name="startValue">Initial value. (optional)</param>
    public static byte GetXor7Bit(byte[] data, int offset = 0, byte startValue = 0x00)
    {
        int value = startValue;
        for (;offset < data.Length; offset++)
            value ^= data[offset];
        value &= 0x7f;
        return (byte)value;
    }
}
