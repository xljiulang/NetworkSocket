using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 定义二进制数据范围
    /// </summary>
    public interface IByteRange
    {
        /// <summary>
        /// 获取偏移量
        /// </summary>
        int Offset { get; }

        /// <summary>
        /// 获取字节数
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 获取字节数组
        /// </summary>
        byte[] Buffer { get; }

        /// <summary>
        /// 分割为大小相等的ByteRange集合
        /// </summary>
        /// <param name="size">新的ByteRange大小</param>
        /// <returns></returns>
        IEnumerable<IByteRange> SplitBySize(int size);
    }
}
