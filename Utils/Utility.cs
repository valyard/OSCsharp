/*
 * @author Paul Varcholik / pvarchol@bespokesoftware.org
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;

namespace OSCsharp.Utils
{
    public static class Utility
    {
        public static T[] CopySubArray<T>(this T[] source, int start, int length)
        {
            T[] result = new T[length];
            Array.Copy(source, start, result, 0, length);

            return result;
        }

        public static byte[] SwapEndian(byte[] data)
        {
            byte[] swapped = new byte[data.Length];
            for (int i = data.Length - 1, j = 0; i >= 0; i--, j++)
            {
                swapped[j] = data[i];
            }

            return swapped;
        }
    }
}