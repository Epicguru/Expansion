using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;

namespace Engine.IO
{
    internal static class IOUtils
    {
        private static Dictionary<int, byte[]> arrays = new Dictionary<int, byte[]>();
        internal static byte[] GetArray(int length)
        {
            if (arrays.ContainsKey(length))
            {
                var arr = arrays[length];
                return arr;
            }
            else
            {
                byte[] arr = new byte[length];
                arrays.Add(length, arr);
                return arr;
            }
        }        
    }
}
