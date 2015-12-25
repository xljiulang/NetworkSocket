using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 定义会话收到的所有数据流接口
    /// </summary>
    public interface IReceiveBuffer
    {
        /// <summary>
        /// 获取同步锁对象
        /// </summary>
        object SyncRoot { get; }

        /// <summary>
        /// 获取用字节表示的流长度
        /// </summary>
        int Length { get; }

        /// <summary>
        /// 获取或设置流中的当前位置
        /// </summary>
        int Position { get; set; }

        /// <summary>
        /// 获取或设置字节存储次序
        /// 默认为Endians.Big
        /// </summary>
        Endians Endian { get; set; }

        /// <summary>
        /// 获取指定位置的字节
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns></returns>
        byte this[int index] { get; }

        /// <summary>
        /// 从流中读取一个字节，并将流内的位置向前推进一个字节
        /// </summary>
        /// <returns></returns>
        bool ReadBoolean();

        /// <summary>
        /// 从流中读取一个字节，并将流内的位置向前推进一个字节，如果已到达流的末尾，则返回 -1
        /// </summary>
        /// <returns></returns>
        byte ReadByte();

        /// <summary>
        /// 从流中读取2个字节，并将流内的位置向前推进2个字节，
        /// 返回其Int16表示类型
        /// </summary>
        /// <returns></returns>
        short ReadInt16();

        /// <summary>
        /// 从流中读取2个字节，并将流内的位置向前推进2个字节，
        /// 返回其UInt16表示类型
        /// </summary>
        /// <returns></returns>
        uint ReadUInt16();

        /// <summary>
        /// 从流中读取4个字节，并将流内的位置向前推进4个字节，
        /// 返回其Int32表示类型
        /// </summary>
        /// <returns></returns>
        int ReadInt32();

        /// <summary>
        /// 从流中读取4个字节，并将流内的位置向前推进4个字节，
        /// 返回其UInt32表示类型
        /// </summary>
        /// <returns></returns>
        uint ReadUInt32();

        /// <summary>
        /// 从流中读取8个字节，并将流内的位置向前推进8个字节，
        /// 返回其Int64表示类型
        /// </summary>
        /// <returns></returns>
        long ReadInt64();

        /// <summary>
        /// 从流中读取8个字节，并将流内的位置向前推进8个字节，
        /// 返回其UInt64表示类型
        /// </summary>
        /// <returns></returns>
        ulong ReadUInt64();

        /// <summary>
        /// 从流中读取到末尾的字节，并将流内的位置向前推进相同的字节
        /// </summary>
        /// <returns></returns>
        byte[] ReadArray();

        /// <summary>
        /// 从流中读取count字节，并将流内的位置向前推进count字节
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        byte[] ReadArray(int count);

        /// <summary>
        /// 从流中读取Position到末尾的所有字节，并将流内的位置推到末尾
        /// 返回以指定编码转换的字符串
        /// </summary>
        /// <param name="encode"></param>
        /// <returns></returns>
        string ReadString(Encoding encode);

        /// <summary>
        /// 从流中读取count字节，并将流内的位置向前推进count字节
        /// 返回以指定编码转换的字符串
        /// </summary>
        /// <param name="count"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        string ReadString(int count, Encoding encode);

        /// <summary>
        /// 从流中读取count字节的范围标记
        /// 并将流内的位置向前推进count个字节
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        IByteRange ReadByteRange(int count);

        /// <summary>
        /// 清空所有数据    
        /// 等同SetLength
        /// </summary>
        void Clear();

        /// <summary>
        /// 从开始位置清除数据        
        /// </summary>
        /// <param name="count">清除的字节数</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        void Clear(int count);

        /// <summary>
        /// 从开始位置将指定长度的数据复制到目标数组
        /// </summary>
        /// <param name="dstArray">目标数组</param>     
        /// <param name="count">要复制的字节数</param>
        void CopyTo(byte[] dstArray, int count);

        /// <summary>
        /// 从开始位置将指定长度的数据复制到目标数组
        /// </summary>
        /// <param name="dstArray">目标数组</param>
        /// <param name="dstOffset">目标数组偏移量</param>
        /// <param name="count">要复制的字节数</param>
        void CopyTo(byte[] dstArray, int dstOffset, int count);

        /// <summary>
        /// 从指定偏移位置将数据复制到目标数组
        /// </summary>
        /// <param name="srcOffset">偏移量</param>
        /// <param name="dstArray">目标数组</param>
        /// <param name="dstOffset">目标数组偏移量</param>
        /// <param name="count">要复制的字节数</param>
        void CopyTo(int srcOffset, byte[] dstArray, int dstOffset, int count);

        /// <summary>
        /// 从Position位置开始查找第一个匹配的值
        /// 返回相对于Position的偏移量
        /// </summary>
        /// <param name="binary">要匹配的数据</param>
        /// <returns></returns>
        int IndexOf(byte[] binary);

        /// <summary>
        /// 将当前流的长度设为指定值
        /// </summary>
        /// <param name="length">长度</param>
        void SetLength(int length);
    }
}
