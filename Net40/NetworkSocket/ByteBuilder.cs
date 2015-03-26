using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace NetworkSocket
{
    /// <summary>
    /// 可变长byte集合 
    /// 非线程安全类型
    /// 多线程下请锁住自身的SyncRoot字段
    /// </summary>
    [DebuggerDisplay("Length = {Length}")]
    public sealed class ByteBuilder
    {
        /// <summary>
        /// 获取容量
        /// </summary>
        public int Capacity { get; private set; }

        /// <summary>
        /// 获取有效数量长度
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// 获取同步锁
        /// </summary>
        public object SyncRoot { get; private set; }

        /// <summary>
        /// 获取原始数据
        /// </summary>
        public byte[] Source { get; private set; }

        /// <summary>
        /// 可变长byte集合
        /// 默认容量是1024byte
        /// </summary>
        public ByteBuilder()
            : this(1024)
        {
        }

        /// <summary>
        /// 可变长byte集合
        /// </summary>
        /// <param name="capacity">容量[乘2倍数增长]</param>
        public ByteBuilder(int capacity)
        {
            this.Capacity = capacity;
            this.Source = new byte[capacity];
            this.SyncRoot = new object();
        }

        /// <summary>
        /// 添加一个bool类型
        /// </summary>
        /// <param name="value">值</param>
        public void Add(bool value)
        {
            this.Add(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// 添加一个字节
        /// </summary>
        /// <param name="value">字节</param>
        public void Add(byte value)
        {
            this.Add(new byte[] { value });
        }

        /// <summary>
        /// 将16位整数转换为byte数组再添加
        /// </summary>
        /// <param name="value">整数</param>
        /// <param name="endian">高低位</param>
        public void Add(short value, Endians endian)
        {
            var bytes = ByteConverter.ToBytes(value, endian);
            this.Add(bytes);
        }

        /// <summary>
        /// 将16位整数转换为byte数组再添加
        /// </summary>
        /// <param name="value">整数</param>
        /// <param name="endian">高低位</param>
        public void Add(ushort value, Endians endian)
        {
            var bytes = ByteConverter.ToBytes(value, endian);
            this.Add(bytes);
        }

        /// <summary>
        /// 将32位整数转换为byte数组再添加
        /// </summary>
        /// <param name="value">整数</param>
        /// <param name="endian">高低位</param>
        public void Add(int value, Endians endian)
        {
            var bytes = ByteConverter.ToBytes(value, endian);
            this.Add(bytes);
        }

        /// <summary>
        /// 将32位整数转换为byte数组再添加
        /// </summary>
        /// <param name="value">整数</param>
        /// <param name="endian">高低位</param>
        public void Add(uint value, Endians endian)
        {
            var bytes = ByteConverter.ToBytes(value, endian);
            this.Add(bytes);
        }

        /// <summary>
        /// 将64位整数转换为byte数组再添加
        /// </summary>
        /// <param name="value">整数</param>
        /// <param name="endian">高低位</param>
        public void Add(long value, Endians endian)
        {
            var bytes = ByteConverter.ToBytes(value, endian);
            this.Add(bytes);
        }

        /// <summary>
        /// 将64位整数转换为byte数组再添加
        /// </summary>
        /// <param name="value">整数</param>
        /// <param name="endian">高低位</param>
        public void Add(ulong value, Endians endian)
        {
            var bytes = ByteConverter.ToBytes(value, endian);
            this.Add(bytes);
        }

        /// <summary>
        /// 将指定数据源的数据添加到集合
        /// </summary>
        /// <param name="value">数据源</param>
        /// <returns></returns>
        public void Add(byte[] value)
        {
            if (value == null || value.Length == 0)
            {
                return;
            }
            this.Add(value, 0, value.Length);
        }

        /// <summary>
        /// 将指定数据源的数据添加到集合
        /// </summary>
        /// <param name="value">数据源</param>
        /// <param name="index">数据源的起始位置</param>
        /// <param name="length">复制的长度</param>
        public void Add(byte[] value, int index, int length)
        {
            if (value == null || value.Length == 0)
            {
                return;
            }

            int newLength = this.Length + length;
            if (newLength > this.Capacity)
            {
                while (newLength > this.Capacity)
                {
                    this.Capacity = this.Capacity * 2;
                }

                byte[] newBuffer = new byte[this.Capacity];
                Buffer.BlockCopy(this.Source, 0, newBuffer, 0, this.Source.Length);
                this.Source = newBuffer;
            }
            Buffer.BlockCopy(value, index, this.Source, this.Length, length);
            this.Length = newLength;
        }


        /// <summary>
        /// 从0位置删除指定长度的字节
        /// </summary>
        /// <param name="length">长度</param>
        public void Remove(int length)
        {
            this.Length = this.Length - length;
            Buffer.BlockCopy(this.Source, length, this.Source, 0, this.Length);
        }


        /// <summary>
        /// 从0位置将数据复制到指定数组
        /// </summary>
        /// <param name="destArray">目标数组</param>
        /// <param name="index">目标数据索引</param>
        /// <param name="length">复制长度</param>
        public void CopyTo(byte[] destArray, int index, int length)
        {
            Buffer.BlockCopy(this.Source, 0, destArray, index, length);
        }


        /// <summary>
        /// 从0位置将数据剪切到指定数组
        /// </summary>
        /// <param name="destArray">目标数组</param>
        /// <param name="index">目标数据索引</param>
        /// <param name="length">剪切长度</param>
        public void CutTo(byte[] destArray, int index, int length)
        {
            this.CopyTo(destArray, index, length);
            this.Remove(length);
        }

        /// <summary>
        /// 返回指定位置的一个字节并转换为bool类型
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool ToBoolean(int index)
        {
            return this.Source[index] != 0;
        }

        /// <summary>
        /// 返回指定位置的字节
        /// </summary>
        /// <param name="index">索引位置</param>
        /// <returns></returns>
        public byte ToByte(int index)
        {
            return this.Source[index];
        }

        /// <summary>
        /// 读取指定位置2个字节，返回其Int16表示类型
        /// </summary>
        /// <param name="index">字节所在索引</param>
        /// <param name="endian">高低位</param>
        /// <returns></returns>
        public int ToInt16(int index, Endians endian)
        {
            return ByteConverter.ToInt16(this.Source, index, endian);
        }

        /// <summary>
        /// 读取指定位置2个字节，返回其UInt16表示类型
        /// </summary>
        /// <param name="index">字节所在索引</param>
        /// <param name="endian">高低位</param>
        /// <returns></returns>
        public uint ToUInt16(int index, Endians endian)
        {
            return ByteConverter.ToUInt16(this.Source, index, endian);
        }

        /// <summary>
        /// 读取指定位置4个字节，返回其Int32表示类型
        /// </summary>
        /// <param name="index">字节所在索引</param>
        /// <param name="endian">高低位</param>
        /// <returns></returns>
        public int ToInt32(int index, Endians endian)
        {
            return ByteConverter.ToInt32(this.Source, index, endian);
        }

        /// <summary>
        /// 读取指定位置4个字节，返回其UInt32表示类型
        /// </summary>
        /// <param name="index">字节所在索引</param>
        /// <param name="endian">高低位</param>
        /// <returns></returns>
        public uint ToUInt32(int index, Endians endian)
        {
            return ByteConverter.ToUInt32(this.Source, index, endian);
        }

        /// <summary>
        /// 读取指定位置8个字节，返回其Int64表示类型
        /// </summary>
        /// <param name="index">字节所在索引</param>
        /// <param name="endian">高低位</param>
        /// <returns></returns>
        public long ToInt64(int index, Endians endian)
        {
            return ByteConverter.ToInt64(this.Source, index, endian);
        }

        /// <summary>
        /// 读取指定位置8个字节，返回其UInt64表示类型
        /// </summary>
        /// <param name="index">字节所在索引</param>
        /// <param name="endian">高低位</param>
        /// <returns></returns>
        public ulong ToUInt64(int index, Endians endian)
        {
            return ByteConverter.ToUInt64(this.Source, index, endian);
        }

        /// <summary>
        /// 返回有效的数据
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            return this.ToArray(0, this.Length);
        }

        /// <summary>
        /// 返回指定长度的数据
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="length">长度</param>
        /// <returns></returns>
        public byte[] ToArray(int index, int length)
        {
            byte[] buffer = new byte[length];
            Buffer.BlockCopy(this.Source, index, buffer, 0, length);
            return buffer;
        }

        /// <summary>
        /// 返回有效数据，并清空所有数据
        /// </summary>
        /// <returns></returns>
        public byte[] ToArrayThenClear()
        {
            var bytes = this.ToArray();
            this.Clear();
            return bytes;
        }

        /// <summary>
        /// 清空数据 
        /// 容量不受到影响
        /// </summary>
        /// <returns></returns>
        public void Clear()
        {
            this.Length = 0;
        }
    }
}
