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
    [DebuggerDisplay("ApiName = {ApiName}")]
    public sealed class FastPacket : PacketBase
    {
        /// <summary>
        /// 获取封包的字节长度
        /// </summary>
        public int TotalBytes { get; private set; }

        /// <summary>
        /// 获取api名称长度
        /// </summary>
        public byte ApiNameLength { get; private set; }

        /// <summary>
        /// 获取api名称
        /// </summary>
        public string ApiName { get; private set; }

        /// <summary>
        /// 获取封包的唯一标识
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// 获取是否为客户端的封包
        /// </summary>
        public bool IsFromClient { get; private set; }

        /// <summary>
        /// 获取或设置是否异常数据
        /// </summary>
        public bool IsException { get; set; }

        /// <summary>
        /// 获取或设置数据体的数据
        /// </summary>
        public byte[] Body { get; set; }

        /// <summary>
        /// 通讯协议的封包
        /// </summary>
        /// <param name="api">api名称</param>
        /// <param name="id">标识符</param>
        /// <param name="fromClient">是否为客户端的封包</param>
        public FastPacket(string api, long id, bool fromClient)
        {
            if (string.IsNullOrEmpty(api))
            {
                throw new ArgumentNullException("api");
            }
            this.ApiName = api;
            this.Id = id;
            this.IsFromClient = fromClient;
        }


        /// <summary>
        /// 将参数序列化并写入为Body
        /// </summary>
        /// <param name="serializer">序列化工具</param>
        /// <param name="parameters">参数</param>
        public void SetBodyParameters(ISerializer serializer, params object[] parameters)
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
        public List<byte[]> GetBodyParameters()
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
            var apiNameBytes = Encoding.UTF8.GetBytes(this.ApiName);
            var headLength = apiNameBytes.Length + 15;

            this.ApiNameLength = (byte)apiNameBytes.Length;
            this.TotalBytes = this.Body == null ? headLength : headLength + this.Body.Length;

            var builder = new ByteBuilder(Endians.Big, this.TotalBytes);
            builder.Add(this.TotalBytes);
            builder.Add(this.ApiNameLength);
            builder.Add(apiNameBytes);
            builder.Add(this.Id);
            builder.Add(this.IsFromClient);
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
            if (builder.Length < 4)
            {
                return null;
            }

            // 包长
            builder.Position = 0;
            var totalBytes = builder.ReadInt32();

            if (builder.Length < totalBytes)
            {
                return null;
            }

            // api名称数据长度
            var apiNameLength = builder.ReadByte();
            // api名称数据
            var apiNameBytes = builder.ReadArray(apiNameLength);
            // 标识符
            var id = builder.ReadInt64();
            // 是否为客户端封包
            var isFromClient = builder.ReadBoolean();
            // 是否异常
            var isException = builder.ReadBoolean();
            // 实体数据
            var body = builder.ReadArray(totalBytes - builder.Position);

            // 清空本条数据
            builder.Remove(totalBytes);

            var apiName = Encoding.UTF8.GetString(apiNameBytes);
            var packet = new FastPacket(apiName, id, isFromClient)
            {
                TotalBytes = totalBytes,
                ApiNameLength = apiNameLength,
                IsException = isException,
                Body = body
            };
            return packet;
        }

        /// <summary>
        /// 字符串显示
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ApiName;
        }
    }
}
