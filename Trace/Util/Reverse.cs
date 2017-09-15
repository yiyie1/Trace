using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trace.Util
{
    public class Reverse
    {
        //翻转byte数组
        public static void ReverseBytes(byte[] bytes)
        {
            byte tmp;
            int len = bytes.Length;

            for (int i = 0; i < len / 2; i++)
            {
                tmp = bytes[len - 1 - i];
                bytes[len - 1 - i] = bytes[i];
                bytes[i] = tmp;
            }
        }

        //规定转换起始位置和长度
        public static void ReverseBytes(byte[] bytes, int start, int len)
        {
            int end = start + len - 1;
            byte tmp;
            int i = 0;
            for (int index = start; index < start + len / 2; index++, i++)
            {
                tmp = bytes[end - i];
                bytes[end - i] = bytes[index];
                bytes[index] = tmp;
            }
        }

        // 翻转字节顺序 (16-bit)
        public static UInt16 ReverseBytes(UInt16 value)
        {
            return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }


        // 翻转字节顺序 (32-bit)
        public static UInt32 ReverseBytes(UInt32 value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        // 翻转字节顺序 (64-bit)
        public static UInt64 ReverseBytes(UInt64 value)
        {
            return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
                   (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 |
                   (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
                   (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
        }
    }
}
