using System.Collections.Generic;

namespace Ahlzen.SysexSharp.SysexLib.Utils;

internal class ByteArrayComparer : IEqualityComparer<byte[]>
{
    public bool Equals(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            return false;
        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[i])
                return false;
        return true;
    }
    public int GetHashCode(byte[] arr)
    {
        // Sum up the elements. Maybe not the optimal hash, but we
        // just need something reasonable.
        int hashCode = 0;
        for (int i = 0; i < arr.Length; i++)
            unchecked { hashCode = hashCode + arr[i]; }
        return hashCode;
    }
}
