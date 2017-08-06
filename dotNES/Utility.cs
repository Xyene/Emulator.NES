using System.Runtime.CompilerServices;

namespace dotNES
{
    static class Utility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe byte AsByte(this bool to) => *((byte*)&to);

        public static void Fill<T>(this T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = value;
            }
        }
    }
}
