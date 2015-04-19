using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 表示位
    /// 值的0和1或false和true
    /// </summary>
    [DebuggerDisplay("{value}")]
    public struct Bit : IComparable<Bit>
    {
        /// <summary>
        /// 值
        /// </summary>
        private byte value;

        /// <summary>
        /// 表示位
        /// </summary>
        /// <param name="value">位值</param>
        private Bit(bool value)
        {
            this.value = (byte)(value ? 1 : 0);
        }

        /// <summary>
        /// 表示位
        /// </summary>
        /// <param name="value">位值</param>
        private Bit(byte value)
        {
            this.value = (byte)(value == 0 ? 0 : 1);
        }

        /// <summary>
        /// 表示位
        /// </summary>
        /// <param name="value">位值</param>
        private Bit(short value)
        {
            this.value = (byte)(value == 0 ? 0 : 1);
        }

        /// <summary>
        /// 表示位
        /// </summary>
        /// <param name="value">位值</param>
        private Bit(ushort value)
        {
            this.value = (byte)(value == 0 ? 0 : 1);
        }

        /// <summary>
        /// 表示位
        /// </summary>
        /// <param name="value">位值</param>
        private Bit(int value)
        {
            this.value = (byte)(value == 0 ? 0 : 1);
        }


        /// <summary>
        /// 表示位
        /// </summary>
        /// <param name="value">位值</param>
        private Bit(uint value)
        {
            this.value = (byte)(value == 0 ? 0 : 1);
        }


        /// <summary>
        /// 表示位
        /// </summary>
        /// <param name="value">位值</param>
        private Bit(long value)
        {
            this.value = (byte)(value == 0 ? 0 : 1);
        }

        /// <summary>
        /// 表示位
        /// </summary>
        /// <param name="value">位值</param>
        private Bit(ulong value)
        {
            this.value = (byte)(value == 0 ? 0 : 1);
        }


        /// <summary>
        /// 字符串显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.value.ToString();
        }

        /// <summary>
        /// 获取哈希码
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }

        /// <summary>
        /// 和指定目标是否相等
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return (obj is Bit) && obj.GetHashCode() == this.GetHashCode();
        }

        /// <summary>
        /// 等于
        /// </summary>
        /// <param name="bit1"></param>
        /// <param name="bit2"></param>
        /// <returns></returns>
        public static bool operator ==(Bit bit1, Bit bit2)
        {
            return bit1.value == bit2.value;
        }

        /// <summary>
        /// 不等于
        /// </summary>
        /// <param name="bit1"></param>
        /// <param name="bit2"></param>
        /// <returns></returns>
        public static bool operator !=(Bit bit1, Bit bit2)
        {
            return bit1.value != bit2.value;
        }



        /// <summary>
        /// 从bool类型隐式转换
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator Bit(bool value)
        {
            return new Bit(value);
        }

        /// <summary>
        /// 从byte类型隐式转换
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator Bit(byte value)
        {
            return new Bit(value);
        }

        /// <summary>
        /// 从short类型隐式转换
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator Bit(short value)
        {
            return new Bit(value);
        }

        /// <summary>
        /// 从ushort类型隐式转换
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator Bit(ushort value)
        {
            return new Bit(value);
        }

        /// <summary>
        /// 从int类型隐式转换
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator Bit(int value)
        {
            return new Bit(value);
        }

        /// <summary>
        /// 从uint类型隐式转换
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator Bit(uint value)
        {
            return new Bit(value);
        }

        /// <summary>
        /// 从long类型隐式转换
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator Bit(long value)
        {
            return new Bit(value);
        }

        /// <summary>
        /// 从ulong类型隐式转换
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator Bit(ulong value)
        {
            return new Bit(value);
        }



        /// <summary>
        /// 隐式转换为bool
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        public static implicit operator bool(Bit bit)
        {
            return bit.value == 1 ? true : false;
        }

        /// <summary>
        /// 隐式转换为byte
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        public static implicit operator byte(Bit bit)
        {
            return bit.value;
        }

        /// <summary>
        /// 隐式转换为short
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        public static implicit operator short(Bit bit)
        {
            return Convert.ToInt16(bit.value);
        }

        /// <summary>
        /// 隐式转换为ushort
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        public static implicit operator ushort(Bit bit)
        {
            return Convert.ToUInt16(bit.value);
        }


        /// <summary>
        /// 隐式转换为int
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        public static implicit operator int(Bit bit)
        {
            return Convert.ToInt32(bit.value);
        }

        /// <summary>
        /// 隐式转换为uint
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        public static implicit operator uint(Bit bit)
        {
            return Convert.ToUInt32(bit.value);
        }

        /// <summary>
        /// 隐式转换为long
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        public static implicit operator long(Bit bit)
        {
            return Convert.ToInt64(bit.value);
        }

        /// <summary>
        /// 隐式转换为ulong
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        public static implicit operator ulong(Bit bit)
        {
            return Convert.ToUInt64(bit.value);
        }

        /// <summary>
        /// 和目标比较
        /// </summary>
        /// <param name="other">目标</param>
        /// <returns></returns>
        int IComparable<Bit>.CompareTo(Bit other)
        {
            if (this == other)
            {
                return 0;
            }
            return this.value == 1 ? 1 : -1;
        }
    }
}
