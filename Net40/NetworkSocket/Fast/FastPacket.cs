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
    public sealed class FastPacket
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
        /// <exception cref="SerializerException"></exception>
        public void SetBodyParameters(ISerializer serializer, params object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                return;
            }
            var builder = new ByteBuilder(Endians.Big);
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
        /// 获取Body的参数值
        /// </summary>
        /// <param name="serializer">序列化工具</param>
        /// <param name="parameterTypes">参数类型</param>
        /// <returns></returns>
        public object[] GetBodyParameters(ISerializer serializer, Type[] parameterTypes)
        {
            var bodyParameters = this.GetBodyParameters();
            var parameters = new object[bodyParameters.Count];

            for (var i = 0; i < bodyParameters.Count; i++)
            {
                var parameterBytes = bodyParameters[i];
                var parameterType = parameterTypes[i];

                if (parameterBytes == null || parameterBytes.Length == 0)
                {
                    parameters[i] = parameterType.IsValueType ? Activator.CreateInstance(parameterType) : null;
                }
                else
                {
                    parameters[i] = serializer.Deserialize(parameterBytes, parameterType);
                }
            }
            return parameters;
        }

        /// <summary>
        /// 将Body的数据解析为参数
        /// </summary>        
        /// <returns></returns>
        private List<byte[]> GetBodyParameters()
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
        /// 转换为ByteRange
        /// </summary>
        /// <exception cref="ProtocolException"></exception>
        /// <returns></returns>
        public ByteRange ToByteRange()
        {
            var apiNameBytes = Encoding.UTF8.GetBytes(this.ApiName);
            var headLength = apiNameBytes.Length + 15;
            this.TotalBytes = this.Body == null ? headLength : headLength + this.Body.Length;

            const int packegMaxSize = 10 * 1204 * 1024; // 10M
            if (this.TotalBytes > packegMaxSize)
            {
                throw new ProtocolException("数据包太大");
            }

            this.ApiNameLength = (byte)apiNameBytes.Length;
            var builder = new ByteBuilder(Endians.Big);
            builder.Add(this.TotalBytes);
            builder.Add(this.ApiNameLength);
            builder.Add(apiNameBytes);
            builder.Add(this.Id);
            builder.Add(this.IsFromClient);
            builder.Add(this.IsException);
            builder.Add(this.Body);
            return builder.ToByteRange();
        }


        /// <summary>
        /// 解析一个数据包       
        /// 不足一个封包时返回null
        /// </summary>
        /// <param name="buffer">接收到的历史数据</param>
        /// <exception cref="ProtocolException"></exception>
        /// <returns></returns>
        public static FastPacket From(ReceiveBuffer buffer)
        {
            if (buffer.Length < 4)
            {
                return null;
            }

            buffer.Position = 0;
            var totalBytes = buffer.ReadInt32();
            const int packegMaxSize = 10 * 1204 * 1024; // 10M
            if (totalBytes > packegMaxSize)
            {
                throw new ProtocolException();
            }

            // 少于15字节是异常数据，清除收到的所有数据
            const int packetMinSize = 15;
            if (totalBytes < packetMinSize)
            {
                throw new ProtocolException();
            }

            // 数据包未接收完整
            if (buffer.Length < totalBytes)
            {
                return null;
            }

            // api名称数据长度
            var apiNameLength = buffer.ReadByte();
            if (totalBytes < apiNameLength + packetMinSize)
            {
                throw new ProtocolException();
            }

            // api名称数据
            var apiNameBytes = buffer.ReadArray(apiNameLength);
            // 标识符
            var id = buffer.ReadInt64();
            // 是否为客户端封包
            var isFromClient = buffer.ReadBoolean();
            // 是否异常
            var isException = buffer.ReadBoolean();
            // 实体数据
            var body = buffer.ReadArray(totalBytes - buffer.Position);

            // 清空本条数据
            buffer.Clear(totalBytes);

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
