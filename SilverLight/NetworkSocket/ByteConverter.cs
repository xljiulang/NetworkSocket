using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// byte类型转换工具类
    /// 提供byte和整型之间的转换
    /// </summary>
    public static class ByteConverter
    {
        /// <summary>
        /// 返回由字节数组中指定位置的8个字节转换来的64位有符号整数
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <param name="startIndex">位置</param>    
        /// <param name="littleEndian">是否低位在前</param>
        /// <returns></returns>        
        public static long ToInt64(byte[] bytes, int startIndex, bool littleEndian)
        {
            if (littleEndian)
            {
                int i1 = bytes[startIndex] |
                    (bytes[startIndex + 1] << 8) |
                    (bytes[startIndex + 2] << 16) |
                    (bytes[startIndex + 3] << 24);

                int i2 = bytes[startIndex + 4] |
                    (bytes[startIndex + 5] << 8) |
                    (bytes[startIndex + 6] << 16) |
                    (bytes[startIndex + 7] << 24);

                return (uint)i1 | ((long)i2 << 32);
            }
            else
            {
                int i1 = (bytes[startIndex] << 24) |
                    (bytes[startIndex + 1] << 16) |
                    (bytes[startIndex + 2] << 8) |
                    bytes[startIndex + 3];

                int i2 = (bytes[startIndex + 4] << 24) |
                    (bytes[startIndex + 5] << 16) |
                    (bytes[startIndex + 6] << 8) |
                    bytes[startIndex + 7];

                return (uint)i2 | ((long)i1 << 32);
            }
        }

        /// <summary>
        /// 返回由字节数组中指定位置的8个字节转换来的64位无符号整数
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <param name="startIndex">位置</param>    
        /// <param name="littleEndian">是否低位在前</param>
        /// <returns></returns>        
        public static ulong ToUInt64(byte[] bytes, int startIndex, bool littleEndian)
        {
            return (ulong)ToInt64(bytes, startIndex, littleEndian);
        }


        /// <summary>
        /// 返回由字节数组中指定位置的四个字节转换来的32位有符号整数
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <param name="startIndex">位置</param>    
        /// <param name="littleEndian">是否低位在前</param>
        /// <returns></returns>        
        public static int ToInt32(byte[] bytes, int startIndex, bool littleEndian)
        {
            if (littleEndian)
            {
                return bytes[startIndex] |
                    (bytes[startIndex + 1] << 8) |
                    (bytes[startIndex + 2] << 16) |
                    (bytes[startIndex + 3] << 24);
            }
            else
            {
                return (bytes[startIndex] << 24) |
                    (bytes[startIndex + 1] << 16) |
                    (bytes[startIndex + 2] << 8) |
                    (bytes[startIndex + 3]);
            }
        }

        /// <summary>
        /// 返回由字节数组中指定位置的四个字节转换来的32位无符号整数
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <param name="startIndex">位置</param>    
        /// <param name="littleEndian">是否低位在前</param>
        /// <returns></returns>        
        public static uint ToUInt32(byte[] bytes, int startIndex, bool littleEndian)
        {
            return (uint)ToInt32(bytes, startIndex, littleEndian);
        }

        /// <summary>
        /// 返回由字节数组中指定位置的四个字节转换来的16位有符号整数
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <param name="startIndex">位置</param>    
        /// <param name="littleEndian">是否低位在前</param>
        /// <returns></returns>
        public static short ToInt16(byte[] bytes, int startIndex, bool littleEndian)
        {
            if (littleEndian)
            {
                return (short)(bytes[startIndex] | (bytes[startIndex + 1] << 8));
            }
            else
            {
                return (short)((bytes[startIndex] << 8) | bytes[startIndex + 1]);
            }
        }

        /// <summary>
        /// 返回由字节数组中指定位置的四个字节转换来的16位无符号整数
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <param name="startIndex">位置</param>    
        /// <param name="littleEndian">是否低位在前</param>
        /// <returns></returns>
        public static ushort ToUInt16(byte[] bytes, int startIndex, bool littleEndian)
        {
            return (ushort)ToInt16(bytes, startIndex, littleEndian);
        }

        /// <summary>
        /// 返回由64位有符号整数转换为的字节数组
        /// </summary>
        /// <param name="value">整数</param>    
        /// <param name="littleEndian">是否低位在前</param>
        /// <returns></returns>
        public static byte[] ToBytes(long value, bool littleEndian)
        {
            byte[] bytes = new byte[4];
            if (littleEndian)
            {
                bytes[0] = (byte)(value);
                bytes[1] = (byte)(value >> 8);
                bytes[2] = (byte)(value >> 16);
                bytes[3] = (byte)(value >> 24);
                bytes[4] = (byte)(value >> 32);
                bytes[5] = (byte)(value >> 40);
                bytes[6] = (byte)(value >> 48);
                bytes[7] = (byte)(value >> 56);
            }
            else
            {
                bytes[7] = (byte)(value);
                bytes[6] = (byte)(value >> 8);
                bytes[5] = (byte)(value >> 16);
                bytes[4] = (byte)(value >> 24);
                bytes[3] = (byte)(value >> 32);
                bytes[2] = (byte)(value >> 40);
                bytes[1] = (byte)(value >> 48);
                bytes[0] = (byte)(value >> 56);
            }
            return bytes;
        }

        /// <summary>
        /// 返回由64位无符号整数转换为的字节数组
        /// </summary>
        /// <param name="value">整数</param>    
        /// <param name="littleEndian">是否低位在前</param>
        /// <returns></returns>
        public static byte[] ToBytes(ulong value, bool littleEndian)
        {
            return ToBytes((long)value, littleEndian);
        }

        /// <summary>
        /// 返回由32位有符号整数转换为的字节数组
        /// </summary>
        /// <param name="value">整数</param>    
        /// <param name="littleEndian">是否低位在前</param>
        /// <returns></returns>
        public static byte[] ToBytes(int value, bool littleEndian)
        {
            byte[] bytes = new byte[4];
            if (littleEndian)
            {
                bytes[0] = (byte)(value);
                bytes[1] = (byte)(value >> 8);
                bytes[2] = (byte)(value >> 16);
                bytes[3] = (byte)(value >> 24);
            }
            else
            {
                bytes[3] = (byte)(value);
                bytes[2] = (byte)(value >> 8);
                bytes[1] = (byte)(value >> 16);
                bytes[0] = (byte)(value >> 24);
            }
            return bytes;
        }

        /// <summary>
        /// 返回由32位无符号整数转换为的字节数组
        /// </summary>
        /// <param name="value">整数</param>    
        /// <param name="littleEndian">是否低位在前</param>
        /// <returns></returns>
        public static byte[] ToBytes(uint value, bool littleEndian)
        {
            return ToBytes((int)value, littleEndian);
        }

        /// <summary>
        /// 返回由16位有符号整数转换为的字节数组
        /// </summary>
        /// <param name="value">整数</param>    
        /// <param name="littleEndian">是否低位在前</param>
        /// <returns></returns>
        public static byte[] ToBytes(short value, bool littleEndian)
        {
            byte[] bytes = new byte[2];
            if (littleEndian)
            {
                bytes[0] = (byte)(value);
                bytes[1] = (byte)(value >> 8);
            }
            else
            {
                bytes[1] = (byte)(value);
                bytes[0] = (byte)(value >> 8);
            }
            return bytes;
        }

        /// <summary>
        /// 返回由16位无符号整数转换为的字节数组
        /// </summary>
        /// <param name="value">整数</param>    
        /// <param name="littleEndian">是否低位在前</param>
        /// <returns></returns>
        public static byte[] ToBytes(ushort value, bool littleEndian)
        {
            return ToBytes((short)value, littleEndian);
        }
    }
}
