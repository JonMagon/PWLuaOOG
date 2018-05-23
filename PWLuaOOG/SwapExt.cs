using System;
using System.Runtime.CompilerServices;

namespace PWLuaOOG
{
    internal static class SwapExt
    {
        public static void Swap<T>(this T[] array, int index1, int index2)
        {
            T local = array[index1];
            array[index1] = array[index2];
            array[index2] = local;
        }
    }
}

