using System;

namespace Knom.TimeBox
{
    internal static class ByteHelper
    {
        public static byte[] CutZeros(this byte[] buffer)
        {
            int i = buffer.Length - 1;
            while (buffer[i] == 0)
            {
                --i;
            }

            byte[] temp = new byte[i + 1];
            Array.Copy(buffer, temp, i + 1);
            return temp;
        }
    }
}