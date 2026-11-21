using System;
using System.Collections.Generic;

namespace MonoEngine.MonoUtils
{
    public static class ListEx
    {
        public static void SwapAndRemove<T>(this List<T> list, int index)
        {
            int lastIndex = list.Count - 1;
            if (index < 0 || index > lastIndex)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (index == lastIndex)
            {
                list.RemoveAt(lastIndex);
                return;
            }

            list[index] = list[lastIndex];
            list.RemoveAt(lastIndex);
        }
    }
}