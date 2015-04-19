using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 表示字节的位集合
    /// 与byte类型相互隐式转换
    /// </summary>
    [DebuggerDisplay("{value}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public struct Bits : IComparable<Bits>
    {
        /// <summary>
        /// 字节值
        /// </summary>
        private byte value;

        /// <summary>
        /// 右移运算
        /// </summary>
        /// <param name="count">移动位数</param>
        /// <returns></returns>
        public Bits MoveRight(int count)
        {
            return (byte)(this.value >> count);
        }

        /// <summary>
        /// 左移运算
        /// </summary>
        /// <param name="count">移动位数</param>
        /// <returns></returns>
        public Bits MoveLeft(int count)
        {
            return (byte)(this.value << count);
        }

        /// <summary>
        /// 从高位取指定个位
        /// 相当于右移8-count个单位
        /// </summary>
        /// <param name="count">位的数量</param>
        /// <returns></returns>
        public Bits Take(int count)
        {
            return this.MoveRight(8 - count);
        }

        /// <summary>
        /// 从指定索引位置取指定个位
        /// 相当于先或移index个单位再右移8-count个单位
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="count">位的数量</param>
        /// <returns></returns>
        public Bits Take(int index, int count)
        {
            return this.MoveLeft(index).MoveRight(8 - count);
        }


        /// <summary>
        /// 字节的位集合
        /// </summary>
        /// <param name="value">字节</param>
        private Bits(byte value)
        {
            this.value = value;
        }

        /// <summary>
        /// 获取或设置指定位的值
        /// </summary>
        /// <param name="index">由高到低的位索引(左到右共8位)</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <returns></returns>
        public Bit this[int index]
        {
            get
            {
                if (index < 0 || index > 7)
                {
                    throw new ArgumentOutOfRangeException("index", "index为0到7之间");
                }
                return ((this.value & (128 >> index)) > 0) ? 1 : 0;
            }
            set
            {
                if (index < 0 || index > 7)
                {
                    throw new ArgumentOutOfRangeException("index", "index为0到7之间");
                }

                if (value == 1)
                {
                    this.value = (byte)(this.value | (128 >> index));
                }
                else
                {
                    this.value = (byte)(this.value & (byte.MaxValue - (128 >> index)));
                }
            }
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
        /// 从byte类型隐式转换
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator Bits(byte value)
        {
            return new Bits(value);
        }

        /// <summary>
        /// 隐式转换为byte类型
        /// </summary>
        /// <param name="bits"></param>
        /// <returns></returns>
        public static implicit operator byte(Bits bits)
        {
            return bits.value;
        }

        /// <summary>
        /// 从位数据获取字节的位集合
        /// 索引为从左边最高位到右边最低位顺序
        /// </summary>
        /// <param name="bit0">索引0位</param>
        /// <param name="bit1">索引1位</param>
        /// <param name="bit2">索引2位</param>
        /// <param name="bit3">索引3位</param>
        /// <param name="bit4">索引4位</param>
        /// <param name="bit5">索引5位</param>
        /// <param name="bit6">索引6位</param>
        /// <param name="bit7">索引7位</param>
        /// <returns></returns>
        public static Bits From(Bit bit0, Bit bit1, Bit bit2, Bit bit3, Bit bit4, Bit bit5, Bit bit6, Bit bit7)
        {
            return Bits.FromBitArray(bit0, bit1, bit2, bit3, bit4, bit5, bit6, bit7);
        }

        /// <summary>
        /// 从位数组转换得到字节的位集合
        /// </summary>
        /// <param name="bitArray">位数组</param>
        /// <returns></returns>
        private static Bits FromBitArray(params Bit[] bitArray)
        {
            Bits bits = 0;
            for (var i = 0; i < bitArray.Length; i++)
            {
                bits[i] = bitArray[i];
            }
            return bits;
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
        /// 比较是否和目标相等
        /// </summary>
        /// <param name="obj">目标</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            return (obj is Bits) && obj.GetHashCode() == this.GetHashCode();
        }


        /// <summary>
        /// 比较
        /// </summary>
        /// <param name="other">目标</param>
        /// <returns></returns>
        int IComparable<Bits>.CompareTo(Bits other)
        {
            if (this == other)
            {
                return 0;
            }

            if (this.value > other.value)
            {
                return 1;
            }

            return -1;
        }

        /// <summary>
        /// Bits类型调试视图
        /// </summary>
        private class DebugView
        {
            /// <summary>
            /// Bits对象
            /// </summary>
            private Bits bits;

            /// <summary>
            /// 调试视图
            /// </summary>
            /// <param name="bits">Bits对象</param>
            public DebugView(Bits bits)
            {
                this.bits = bits;
            }

            /// <summary>
            /// 查看的内容
            /// </summary>
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Bit[] Keys
            {
                get
                {
                    var bitArray = new Bit[8];
                    for (var i = 0; i < bitArray.Length; i++)
                    {
                        bitArray[i] = this.bits[i];
                    }
                    return bitArray;
                }
            }
        }
    }
}
