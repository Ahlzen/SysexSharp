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
    }
}
