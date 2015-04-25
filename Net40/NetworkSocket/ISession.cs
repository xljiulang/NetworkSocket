using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 定义会话接口
    /// </summary>   
    public interface ISession
    {
        /// <summary>
        /// 获取用户数据字典
        /// </summary>
        ITag TagData { get; }

        /// <summary>
        /// 获取用户数据字典
        /// </summary>
        dynamic TagBag { get; }

        /// <summary>
        /// 获取远程终结点
        /// </summary>
        IPEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// 获取是否已连接到远程端
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 异步发送数据
        /// </summary>
        /// <param name="byteRange">数据范围</param>  
        void Send(ByteRange byteRange);

        /// <summary>      
        /// 断开和远程端的连接
        /// </summary>
        void Close();
    }
}
