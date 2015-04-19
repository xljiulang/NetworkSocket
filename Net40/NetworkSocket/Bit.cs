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
    public struct Bit : IConvertible, IComparable<Bit>
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
        /// 获取对象类型
        /// </summary>
        /// <returns></returns>
        public TypeCode GetTypeCode()
        {
            return TypeCode.Boolean;
        }

        /// <summary>
        /// 转换为bool类型
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public bool ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(this.value);
        }

        /// <summary>
        /// 转换为byte类型
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public byte ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(this.value);
        }

        /// <summary>
        /// 转换为cahr类型
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public char ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(this.value);
        }

        /// <summary>
        /// 转换为DateTime类型
        /// </summary>
        /// <param name="provider"></param>
        /// <exception cref="NotImplementedException"></exception>
        /// <returns></returns>
        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 转换为Decimal类型
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public decimal ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(this.value);
        }

        /// <summary>
        /// 转换为Double类型
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public double ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(this.value);
        }

        /// <summary>
        /// 转换为Int16类型
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public short ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(this.value);
        }

        /// <summary>
        /// 转换为Int32类型
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public int ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(this.value);
        }

        /// <summary>
        /// 转换为Int64类型
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public long ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(this.value);
        }

        /// <summary>
        /// 转换为SByte类型
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public sbyte ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(this.value);
        }

        /// <summary>
        /// 转换为Single类型
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public float ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(this.value);
        }

        /// <summary>
        /// 转换为String类型
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public string ToString(IFormatProvider provider)
        {
            return this.value.ToString(provider);
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
        /// 转换为指定类型
        /// </summary>
        /// <param name="conversionType"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public object ToType(Type conversionType, IFormatProvider provider)
        {
            var method = typeof(Bit).GetMethod("To" + conversionType.Name);
            if (method == null)
            {
                return null;
            }
            return method.Invoke(null, null);
        }

        /// <summary>
        /// 转换为UInt16类型
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public ushort ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(this.value);
        }

        /// <summary>
        /// 转换为UInt32类型
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public uint ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(this.value);
        }

        /// <summary>
        /// 转换为UInt64类型
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public ulong ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(this.value);
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
        /// 隐式转换为short
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
            return bit.ToInt16(null);
        }

        /// <summary>
        /// 隐式转换为ushort
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        public static implicit operator ushort(Bit bit)
        {
            return bit.ToUInt16(null);
        }


        /// <summary>
        /// 隐式转换为int
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        public static implicit operator int(Bit bit)
        {
            return bit.ToInt32(null);
        }

        /// <summary>
        /// 隐式转换为uint
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        public static implicit operator uint(Bit bit)
        {
            return bit.ToUInt32(null);
        }

        /// <summary>
        /// 隐式转换为long
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        public static implicit operator long(Bit bit)
        {
            return bit.ToInt64(null);
        }

        /// <summary>
        /// 隐式转换为ulong
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        public static implicit operator ulong(Bit bit)
        {
            return bit.ToUInt64(null);
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
