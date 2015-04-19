using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 可变长byte集合 
    /// 非线程安全类型
    /// 多线程下请锁住自身的SyncRoot字段
    /// </summary>
    [DebuggerDisplay("Length = {Length}")]
    public class ByteBuilder
    {
        /// <summary>
        /// 当前所有所数据
        /// </summary>
        private byte[] buffer;

        /// <summary>
        /// 获取字节存储次序枚举
        /// </summary>
        public Endians Endian { get; private set; }

        /// <summary>
        /// 获取容量
        /// </summary>
        public int Capacity { get; private set; }

        /// <summary>
        /// 获取有效数量长度
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// 获取或设置指针位置
        /// Read相关操作时变化
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// 获取同步锁
        /// </summary>
        public object SyncRoot { get; private set; }


        /// <summary>
        /// 可变长byte集合
        /// 默认容量是1024byte，高位在前低位在后
        /// </summary>
        public ByteBuilder()
            : this(Endians.Big)
        {
        }

        /// <summary>
        /// 可变长byte集合
        /// 默认容量是1024byte
        /// </summary>
        /// <param name="endian">字节存储次序</param>
        public ByteBuilder(Endians endian)
            : this(endian, 1024)
        {
        }

        /// <summary>
        /// 可变长byte集合
        /// </summary>
        /// <param name="endian">字节存储次序</param>
        /// <param name="capacity">容量[乘2倍数增长]</param>  
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ByteBuilder(Endians endian, int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException("capacity", "capacity必须大于0");
            }
            this.Capacity = capacity;
            this.Endian = endian;
            this.buffer = new byte[capacity];
            this.SyncRoot = new object();
        }

        /// <summary>
        /// 可变长byte集合
        /// </summary>
        /// <param name="endian">字节存储次序</param>
        /// <param name="buffer">数据源</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ByteBuilder(Endians endian, byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("source");
            }
            if (buffer.Length == 0)
            {
                throw new ArgumentOutOfRangeException("source", "source的长度必须大于0");
            }

            this.Endian = endian;
            this.SyncRoot = new object();

            this.Length = this.Capacity = buffer.Length;
            this.buffer = buffer;
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
        public void Add(short value)
        {
            var bytes = ByteConverter.ToBytes(value, this.Endian);
            this.Add(bytes);
        }

        /// <summary>
        /// 将16位整数转换为byte数组再添加
        /// </summary>
        /// <param name="value">整数</param>        
        public void Add(ushort value)
        {
            var bytes = ByteConverter.ToBytes(value, this.Endian);
            this.Add(bytes);
        }

        /// <summary>
        /// 将32位整数转换为byte数组再添加
        /// </summary>
        /// <param name="value">整数</param>        
        public void Add(int value)
        {
            var bytes = ByteConverter.ToBytes(value, this.Endian);
            this.Add(bytes);
        }

        /// <summary>
        /// 将32位整数转换为byte数组再添加
        /// </summary>
        /// <param name="value">整数</param>        
        public void Add(uint value)
        {
            var bytes = ByteConverter.ToBytes(value, this.Endian);
            this.Add(bytes);
        }

        /// <summary>
        /// 将64位整数转换为byte数组再添加
        /// </summary>
        /// <param name="value">整数</param>        
        public void Add(long value)
        {
            var bytes = ByteConverter.ToBytes(value, this.Endian);
            this.Add(bytes);
        }

        /// <summary>
        /// 将64位整数转换为byte数组再添加
        /// </summary>
        /// <param name="value">整数</param>        
        public void Add(ulong value)
        {
            var bytes = ByteConverter.ToBytes(value, this.Endian);
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
        /// <exception cref="ArgumentOutOfRangeException"></exception>
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
                Buffer.BlockCopy(this.buffer, 0, newBuffer, 0, this.buffer.Length);
                this.buffer = newBuffer;
            }
            Buffer.BlockCopy(value, index, this.buffer, this.Length, length);
            this.Length = newLength;
        }


        /// <summary>
        /// 从0位置删除指定长度的字节
        /// </summary>
        /// <param name="length">长度</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void Remove(int length)
        {
            this.Length = this.Length - length;
            Buffer.BlockCopy(this.buffer, length, this.buffer, 0, this.Length);
        }


        /// <summary>
        /// 从0位置将数据复制到指定数组
        /// </summary>
        /// <param name="destArray">目标数组</param>
        /// <param name="index">目标数据索引</param>
        /// <param name="length">复制长度</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void CopyTo(byte[] destArray, int index, int length)
        {
            Buffer.BlockCopy(this.buffer, 0, destArray, index, length);
        }


        /// <summary>
        /// 从0位置将数据剪切到指定数组
        /// </summary>
        /// <param name="destArray">目标数组</param>
        /// <param name="index">目标数据索引</param>
        /// <param name="length">剪切长度</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void CutTo(byte[] destArray, int index, int length)
        {
            this.CopyTo(destArray, index, length);
            this.Remove(length);
        }

        /// <summary>
        /// 返回指定位置的一个字节并转换为bool类型
        /// </summary>
        /// <param name="index">索引</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public bool ToBoolean(int index)
        {
            return this.buffer[index] != 0;
        }

        /// <summary>
        /// 返回一个字节并转换为bool类型
        /// </summary>       
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public bool ReadBoolean()
        {
            var value = this.ToBoolean(this.Position);
            this.Position = this.Position + 1;
            return value;
        }

        /// <summary>
        /// 获取或设置指定位置的字节
        /// </summary>
        /// <param name="index">索引位置</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public byte this[int index]
        {
            get
            {
                if (index < 0 || index >= this.Length)
                {
                    throw new ArgumentOutOfRangeException();
                }
                return this.buffer[index];
            }
            set
            {
                if (index < 0 || index >= this.Length)
                {
                    throw new ArgumentOutOfRangeException();
                }
                this.buffer[index] = value;
            }
        }

        /// <summary>
        /// 返回一个字节
        /// </summary>      
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public byte ReadByte()
        {
            var value = this[this.Position];
            this.Position = this.Position + 1;
            return value;
        }

        /// <summary>
        /// 读取指定位置2个字节，返回其Int16表示类型
        /// </summary>
        /// <param name="index">字节所在索引</param>   
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public short ToInt16(int index)
        {
            return ByteConverter.ToInt16(this.buffer, index, this.Endian);
        }

        /// <summary>
        /// 读取2个字节，返回其Int16表示类型
        /// </summary>     
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public short ReadInt16()
        {
            var value = this.ToInt16(this.Position);
            this.Position = this.Position + 2;
            return value;
        }

        /// <summary>
        /// 读取指定位置2个字节，返回其UInt16表示类型
        /// </summary>
        /// <param name="index">字节所在索引</param> 
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public uint ToUInt16(int index)
        {
            return ByteConverter.ToUInt16(this.buffer, index, this.Endian);
        }

        /// <summary>
        /// 读取2个字节，返回其UInt16表示类型
        /// </summary>      
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public uint ReadUInt16()
        {
            var value = this.ToUInt16(this.Position);
            this.Position = this.Position + 2;
            return value;
        }

        /// <summary>
        /// 读取指定位置4个字节，返回其Int32表示类型
        /// </summary>
        /// <param name="index">字节所在索引</param>  
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public int ToInt32(int index)
        {
            return ByteConverter.ToInt32(this.buffer, index, this.Endian);
        }

        /// <summary>
        /// 读取4个字节，返回其Int32表示类型
        /// </summary>          
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public int ReadInt32()
        {
            var value = this.ToInt32(this.Position);
            this.Position = this.Position + 4;
            return value;
        }

        /// <summary>
        /// 读取指定位置4个字节，返回其UInt32表示类型
        /// </summary>
        /// <param name="index">字节所在索引</param>   
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public uint ToUInt32(int index)
        {
            return ByteConverter.ToUInt32(this.buffer, index, this.Endian);
        }

        /// <summary>
        /// 读取4个字节，返回其UInt32表示类型
        /// </summary>     
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public uint ReadUInt32()
        {
            var value = this.ToUInt32(this.Position);
            this.Position = this.Position + 4;
            return value;
        }

        /// <summary>
        /// 读取指定位置8个字节，返回其Int64表示类型
        /// </summary>
        /// <param name="index">字节所在索引</param>   
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public long ToInt64(int index)
        {
            return ByteConverter.ToInt64(this.buffer, index, this.Endian);
        }

        /// <summary>
        /// 读取8个字节，返回其Int64表示类型
        /// </summary>         
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public long ReadInt64()
        {
            var value = this.ToInt64(this.Position);
            this.Position = this.Position + 8;
            return value;
        }

        /// <summary>
        /// 读取指定位置8个字节，返回其UInt64表示类型
        /// </summary>
        /// <param name="index">字节所在索引</param>    
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public ulong ToUInt64(int index)
        {
            return ByteConverter.ToUInt64(this.buffer, index, this.Endian);
        }

        /// <summary>
        /// 读取8个字节，返回其UInt64表示类型
        /// </summary>        
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public ulong ReadUInt64()
        {
            var value = this.ToUInt64(this.Position);
            this.Position = this.Position + 8;
            return value;
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
        /// 读取有效数据
        /// </summary>
        /// <returns></returns>
        public byte[] ReadArray()
        {
            return this.ReadArray(this.Length - this.Position);
        }

        /// <summary>
        /// 读取有效数据
        /// </summary>
        /// <param name="length">读取长度</param>
        /// <returns></returns>
        public byte[] ReadArray(int length)
        {
            var value = this.ToArray(this.Position, length);
            this.Position = this.Position + length;
            return value;
        }

        /// <summary>
        /// 返回指定长度的数据
        /// </summary>
        /// <param name="index">索引</param>        
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public byte[] ToArray(int index)
        {
            return this.ToArray(index, this.Length - index);
        }

        /// <summary>
        /// 返回指定长度的数据
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="length">长度</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public byte[] ToArray(int index, int length)
        {
            byte[] buffer = new byte[length];
            Buffer.BlockCopy(this.buffer, index, buffer, 0, length);
            return buffer;
        }

        /// <summary>
        /// 清空数据 
        /// 容量不受到影响
        /// </summary>
        /// <returns></returns>
        public void Clear()
        {
            this.Position = 0;
            this.Length = 0;
        }

        /// <summary>
        /// 获取当前缓冲区的数据
        /// </summary>
        /// <returns></returns>
        public byte[] GetBuffer()
        {
            return this.buffer;
        }
    }
}
