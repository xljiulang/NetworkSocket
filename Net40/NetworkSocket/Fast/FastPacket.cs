using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 通讯协议的封包
    /// </summary>
    [DebuggerDisplay("Command = {Command}")]
    public sealed class FastPacket : PacketBase
    {
        /// <summary>
        /// 获取命令值
        /// </summary>
        public int Command { get; private set; }

        /// <summary>
        /// 获取哈希值
        /// </summary>
        public long HashCode { get; private set; }

        /// <summary>
        /// 获取是否异常
        /// </summary>
        public bool IsException { get; private set; }

        /// <summary>
        /// 获取数据体的数据
        /// </summary>
        public byte[] Body { get; private set; }

        /// <summary>
        /// 通讯协议的封包
        /// </summary>
        /// <param name="command">命令值</param>
        /// <param name="hashCode">哈希码</param>
        public FastPacket(int command, long hashCode)
        {
            this.Command = command;
            this.HashCode = hashCode;
        }

        /// <summary>
        /// 通讯协议的封包
        /// </summary>
        /// <param name="command">命令值</param>
        /// <param name="hashCode">哈希码</param>     
        /// <param name="isException">是否为异常包</param>
        /// <param name="body">数据体</param>
        public FastPacket(int command, long hashCode, bool isException, byte[] body)
        {
            this.Command = command;
            this.HashCode = hashCode;
            this.IsException = isException;
            this.Body = body;
        }

        /// <summary>
        /// 设置异常包
        /// 异常项的Message属性文本作为数据包的唯一参数
        /// </summary>
        /// <param name="serializer">序列化工具</param>
        /// <param name="message">异常信息</param>
        internal void SetException(ISerializer serializer, string message)
        {
            this.IsException = true;
            this.SetBodyBinary(serializer, message);
        }

        /// <summary>
        /// 将参数序列化并写入为Body
        /// </summary>
        /// <param name="serializer">序列化工具</param>
        /// <param name="parameters">参数</param>
        public void SetBodyBinary(ISerializer serializer, params object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                return;
            }
            var builder = new ByteBuilder(Endians.Big, 8);
            foreach (var item in parameters)
            {
                // 序列化参数为二进制内容
                var paramBytes = serializer.Serialize(item);
                // 添加参数内容长度            
                builder.Add(paramBytes == null ? 0 : paramBytes.Length);
                // 添加参数内容
                builder.Add(paramBytes);
            }
            this.Body = builder.ToArray();
        }

        /// <summary>
        /// 将Body的数据解析为参数
        /// </summary>        
        /// <returns></returns>
        public List<byte[]> GetBodyParameter()
        {
            var parameterList = new List<byte[]>();

            if (this.Body == null || this.Body.Length < 4)
            {
                return parameterList;
            }

            var index = 0;
            while (index < this.Body.Length)
            {
                // 参数长度
                var length = ByteConverter.ToInt32(this.Body, index, Endians.Big);
                index = index + 4;
                var paramBytes = new byte[length];
                // 复制出参数的数据
                Buffer.BlockCopy(this.Body, index, paramBytes, 0, length);
                index = index + length;
                parameterList.Add(paramBytes);
            }

            return parameterList;
        }


        /// <summary>
        /// 转换为二进制数据
        /// </summary>
        /// <returns></returns>
        public override byte[] ToBytes()
        {
            // 总长度(4) + command(4) + hashCode(8) + IsException(1)
            const int headLength = 17;
            // 总长度
            int totalLength = this.Body == null ? headLength : headLength + this.Body.Length;

            var builder = new ByteBuilder(Endians.Big, totalLength);
            builder.Add(totalLength);
            builder.Add(this.Command);
            builder.Add(this.HashCode);
            builder.Add(this.IsException);
            builder.Add(this.Body);
            return builder.Source;
        }


        /// <summary>
        /// 解析一个数据包       
        /// 不足一个封包时返回null
        /// </summary>
        /// <param name="builder">接收到的历史数据</param>
        /// <returns></returns>
        public static FastPacket From(ByteBuilder builder)
        {
            // 包头长度
            const int headLength = 17;
            // 不会少于17
            if (builder.Length < headLength)
            {
                return null;
            }

            // 包长
            var totalLength = builder.ToInt32(0);
            // 包长要小于等于数据长度
            if (totalLength > builder.Length || totalLength < headLength)
            {
                return null;
            }

            // command
            var command = builder.ToInt32(4);
            // 哈希值
            var hashCode = builder.ToInt64(8);
            // 是否异常
            var isException = builder.ToBoolean(16);
            // 实体数据
            var body = builder.ToArray(17, totalLength - headLength);

            // 清空本条数据
            builder.Remove(totalLength);
            return new FastPacket(command, hashCode, isException, body);
        }

        /// <summary>
        /// 字符串显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Command.ToString();
        }
    }
}
