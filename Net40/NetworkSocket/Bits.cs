using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 表示字节的位集合
    /// 值为byte类型
    /// </summary>
    [Serializable]
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
        public bool this[int index]
        {
            get
            {
                if (index < 0 || index > 7)
                {
                    throw new ArgumentOutOfRangeException("index", "index为0到7之间");
                }
                return ((this.value & (128 >> index)) > 0) ? true : false;
            }
            set
            {
                if (index < 0 || index > 7)
                {
                    throw new ArgumentOutOfRangeException("index", "index为0到7之间");
                }

                if (value == true)
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
            public int[] Keys
            {
                get
                {
                    var bitArray = new int[8];
                    for (var i = 0; i < bitArray.Length; i++)
                    {
                        bitArray[i] = this.bits[i].GetHashCode();
                    }
                    return bitArray;
                }
            }
        }
    }
}
