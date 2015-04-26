using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 表示会话接收到的历史数据
    /// 非线程安全类型
    /// 不可继承
    /// </summary>
    [DebuggerDisplay("Position = {Position}, Length = {Length}, Endian = {Endian}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public sealed class ReceiveBuffer
    {
        /// <summary>
        /// 指针位置
        /// </summary>
        private int _position;

        /// <summary>
        /// 容量
        /// </summary>
        private int _capacity;

        /// <summary>
        /// 当前数据
        /// </summary>
        private byte[] _buffer;

        /// <summary>
        /// 获取同步锁
        /// </summary>
        public readonly object SyncRoot = new object();

        /// <summary>
        /// 获取数据长度
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// 获取或设置字节存储次序
        /// </summary>
        public Endians Endian { get; set; }

        /// <summary>
        /// 获取或设置指针位置    
        /// 为[0, Length]之间
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int Position
        {
            get
            {
                return this._position;
            }
            set
            {
                if (value < 0 || value > this.Length)
                {
                    throw new ArgumentOutOfRangeException();
                }
                this._position = value;
            }
        }


        /// <summary>
        /// 提供二进制数据读取和操作支持
        /// </summary>             
        internal ReceiveBuffer()
        {
            this.Endian = Endians.Big;
            this._capacity = 1024;
            this._buffer = new byte[this._capacity];
        }

        /// <summary>
        /// 添加指定数据数组
        /// </summary>
        /// <param name="array">数组</param>
        /// <param name="offset">数组的偏移量</param>
        /// <param name="count">字节数</param>       
        internal void Add(byte[] array, int offset, int count)
        {
            if (array == null || array.Length == 0)
            {
                return;
            }

            int newLength = this.Length + count;
            this.ExpandCapacity(newLength);

            Buffer.BlockCopy(array, offset, this._buffer, this.Length, count);
            this.Length = newLength;
        }


        /// <summary>
        /// 扩容
        /// </summary>
        /// <param name="newLength">满足的新大小</param>
        private void ExpandCapacity(int newLength)
        {
            if (newLength <= this._capacity)
            {
                return;
            }

            while (newLength > this._capacity)
            {
                this._capacity = this._capacity * 2;
            }

            var newBuffer = new byte[this._capacity];
            if (this.Length > 0)
            {
                Buffer.BlockCopy(this._buffer, 0, newBuffer, 0, this.Length);
            }
            this._buffer = newBuffer;
        }

        /// <summary>
        /// 获取指定位置的字节
        /// </summary>
        /// <param name="index">索引</param>
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
                return this._buffer[index];
            }
        }

        /// <summary>
        /// 从Position偏移位置读取一个字节并转换为bool类型
        /// </summary>       
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public bool ReadBoolean()
        {
            return this.ReadByte() != 0;
        }

        /// <summary>
        /// 从Position偏移位置读取一个字节
        /// </summary>      
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public byte ReadByte()
        {
            var value = this[this.Position];
            this.Position = this.Position + sizeof(byte);
            return value;
        }


        /// <summary>
        /// 从Position偏移位置读取2个字节
        /// 返回其Int16表示类型
        /// </summary>     
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public short ReadInt16()
        {
            var value = ByteConverter.ToInt16(this._buffer, this.Position, this.Endian);
            this.Position = this.Position + sizeof(short);
            return value;
        }

        /// <summary>
        /// 从Position偏移位置读取2个字节
        /// 返回其UInt16表示类型
        /// </summary>      
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public uint ReadUInt16()
        {
            var value = ByteConverter.ToUInt16(this._buffer, this.Position, this.Endian);
            this.Position = this.Position + sizeof(ushort);
            return value;
        }

        /// <summary>
        /// 从Position偏移位置读取4个字节
        /// 返回其Int32表示类型
        /// </summary>          
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public int ReadInt32()
        {
            var value = ByteConverter.ToInt32(this._buffer, this.Position, this.Endian);
            this.Position = this.Position + sizeof(int);
            return value;
        }

        /// <summary>
        /// 从Position偏移位置读取4个字节
        /// 返回其UInt32表示类型
        /// </summary>     
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public uint ReadUInt32()
        {
            var value = ByteConverter.ToUInt32(this._buffer, this.Position, this.Endian);
            this.Position = this.Position + sizeof(uint);
            return value;
        }


        /// <summary>
        /// 从Position偏移位置读取8个字节
        /// 返回其Int64表示类型
        /// </summary>         
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public long ReadInt64()
        {
            var value = ByteConverter.ToInt64(this._buffer, this.Position, this.Endian);
            this.Position = this.Position + sizeof(long);
            return value;
        }

        /// <summary>
        /// 从Position偏移位置读取8个字节
        /// 返回其UInt64表示类型
        /// </summary>        
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public ulong ReadUInt64()
        {
            var value = ByteConverter.ToUInt64(this._buffer, this.Position, this.Endian);
            this.Position = this.Position + sizeof(ulong);
            return value;
        }

        /// <summary>
        /// 从Position偏移位置读取所有数据
        /// </summary>
        /// <returns></returns>
        public byte[] ReadArray()
        {
            return this.ReadArray(this.Length - this.Position);
        }


        /// <summary>
        /// 从Position偏移位置读取指定长度数据
        /// </summary>
        /// <param name="count">要读取的字节数</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public byte[] ReadArray(int count)
        {
            var bytes = new byte[count];
            this.CopyTo(this.Position, bytes, 0, count);
            this.Position = this.Position + count;
            return bytes;
        }

        /// <summary>
        /// 将指定长度的数据复制到目标数组
        /// </summary>
        /// <param name="dstArray">目标数组</param>     
        /// <param name="count">要复制的字节数</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void CopyTo(byte[] dstArray, int count)
        {
            this.CopyTo(dstArray, 0, count);
        }

        /// <summary>
        /// 将指定长度的数据复制到目标数组
        /// </summary>
        /// <param name="dstArray">目标数组</param>
        /// <param name="dstOffset">目标数组偏移量</param>
        /// <param name="count">要复制的字节数</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void CopyTo(byte[] dstArray, int dstOffset, int count)
        {
            this.CopyTo(0, dstArray, dstOffset, count);
        }

        /// <summary>
        /// 从指定偏移位置将数据复制到目标数组
        /// </summary>
        /// <param name="srcOffset">偏移量</param>
        /// <param name="dstArray">目标数组</param>
        /// <param name="dstOffset">目标数组偏移量</param>
        /// <param name="count">要复制的字节数</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void CopyTo(int srcOffset, byte[] dstArray, int dstOffset, int count)
        {
            Buffer.BlockCopy(this._buffer, srcOffset, dstArray, dstOffset, count);
        }

        /// <summary>
        /// 清空所有数据        
        /// </summary>
        /// <returns></returns>
        public void Clear()
        {
            this.Length = 0;
        }

        /// <summary>
        /// 清除数据        
        /// </summary>
        /// <param name="count">清除的字节数</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void Clear(int count)
        {
            if (count < 0 || count > this.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
            var newLength = this.Length - count;
            Buffer.BlockCopy(this._buffer, count, this._buffer, 0, newLength);
            this.Length = newLength;
        }

        /// <summary>
        /// 调试视图
        /// </summary>
        private class DebugView
        {
            /// <summary>
            /// 查看的对象
            /// </summary>
            private ReceiveBuffer view;

            /// <summary>
            /// 调试视图
            /// </summary>
            /// <param name="view">查看的对象</param>
            public DebugView(ReceiveBuffer view)
            {
                this.view = view;
            }

            /// <summary>
            /// 查看的内容
            /// </summary>
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public byte[] Values
            {
                get
                {
                    var array = new byte[view.Length];
                    view.CopyTo(array, view.Length);
                    return array;
                }
            }
        }
    }
}
