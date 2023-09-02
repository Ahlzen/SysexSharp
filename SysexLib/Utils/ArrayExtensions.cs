using System;

namespace Ahlzen.SysexSharp.SysexLib.Utils
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Similar to Substring, returns a slice of an existing array.
        /// </summary>
        /// <remarks>
        /// Adapted from:
        /// https://www.techiedelight.com/get-subarray-of-array-csharp/
        /// </remarks>
        public static T[] SubArray<T>(this T[] source, int offset, int length)
        {
            var target = new T[length];
            Array.Copy(source, offset, target, 0, length);
            return target;
        }

        /// <summary>
        /// Copies elements that are not null from source to dest. Nulls are
        /// skipped in bout source and dest.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="offset">
        /// Offset in dest of first element to be copied.
        /// </param>
        public static void CopyNonNullTo<T>(this T?[] source, T[] dest, int offset = 0) where T: struct
        {
            for (int i = 0; i < source.Length; i++)
                if (source[i] != null)
                    dest[i + offset] = source[i]!.Value;
        }
    }
}
