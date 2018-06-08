using System;

namespace Knom.TimeBox
{
    internal static class ByteHelper
    {
        public static byte[] Append(this byte[] buffer, byte[] buffer2)
        {
            int length = buffer.Length + buffer2.Length;

            byte[] result = new byte[length];

            Array.Copy(buffer, result, buffer.Length);
            Array.Copy(buffer2, 0, result, buffer.Length, buffer2.Length);

            return result;
        }
    }
}