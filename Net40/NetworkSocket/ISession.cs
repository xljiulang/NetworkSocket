using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 定义会话的接口
    /// </summary>   
    public interface ISession : IDisposable
    {
        /// <summary>
        /// 获取用户数据字典
        /// </summary>
        ITag Tag { get; }

        /// <summary>
        /// 获取本机终结点
        /// </summary>
        IPEndPoint LocalEndPoint { get; }

        /// <summary>
        /// 获取远程终结点
        /// </summary>
        IPEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// 获取是否已连接到远程端
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 获取会话的协议名
        /// </summary>
        string Protocol { get; }

        /// <summary>
        /// 获取会话的包装对象
        /// 该对象一般为会话对协议操作的包装
        /// </summary>
        IWrapper Wrapper { get; }

        /// <summary>
        /// 获取会话的协议是否和protocol匹配
        /// </summary>
        /// <param name="protocol">协议名</param>
        /// <returns></returns>
        bool? IsProtocol(string protocol);

        /// <summary>
        /// 异步发送数据
        /// </summary>
        /// <param name="byteRange">数据范围</param>  
        void Send(IByteRange byteRange);

        /// <summary>      
        /// 断开和远程端的连接
        /// </summary>
        void Close();

        /// <summary>
        /// 设置会话的协议名和会话包装对象
        /// </summary>
        /// <param name="protocol">协议</param>
        /// <param name="wrapper">会话的包装对象</param>
        void SetProtocolWrapper(string protocol, IWrapper wrapper);
    }
}
