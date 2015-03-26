using NetworkSocket.Fast.Attributes;
using NetworkSocket.Fast.Exceptions;
using NetworkSocket.Fast.Methods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 快速构建Tcp客户端抽象类
    /// </summary>
    public abstract class FastTcpClientBase : TcpClientBase<FastPacket>
    {
        /// <summary>
        /// 所有服务方法
        /// </summary>
        private List<ServiceMethod> serverMethods;

        /// <summary>
        /// 获取或设置序列化工具
        /// 默认是Json序列化
        /// </summary>
        public ISerializer Serializer { get; set; }

        /// <summary>
        /// 快速构建Tcp服务端
        /// </summary>
        public FastTcpClientBase()
        {
            var methods = this.GetType().GetMethods().Where(item => Attribute.IsDefined(item, typeof(ServiceAttribute)));
            this.serverMethods = methods.Select(item => new ServiceMethod(item)).ToList();
            this.Serializer = new DefaultSerializer();
        }

        /// <summary>
        /// 当接收到远程端的数据时，将触发此方法
        /// 此方法用于处理和分析收到的数据
        /// 如果得到一个数据包，将触发OnRecvComplete方法
        /// [注]这里只需处理一个数据包的流程
        /// </summary>
        /// <param name="recvBuilder">接收到的历史数据</param>
        /// <returns>如果不够一个数据包，则请返回null</returns>
        protected override FastPacket OnReceive(ByteBuilder recvBuilder)
        {
            return FastPacket.GetPacket(recvBuilder);
        }

        /// <summary>
        /// 当接收到服务发来的数据包时，将触发此方法
        /// </summary>
        /// <param name="packet">数据包</param>
        protected override void OnRecvComplete(FastPacket packet)
        {
            if (packet.IsException == true)
            {
                var bytes = packet.GetBodyParameter().FirstOrDefault();
                var exceptionCallBack = CallbackTable.Take(packet.HashCode);

                if (exceptionCallBack != null)
                {
                    exceptionCallBack(packet.IsException, bytes);
                }
                else
                {
                    this.OnException(this.Serializer.Deserialize(bytes, typeof(RemoteException)) as RemoteException);
                }
                return;
            }

            var method = this.serverMethods.FirstOrDefault(item => item.ServiceAttribute.Command == packet.Command);
            if (method == null)
            {
                var exception = new Exception("相关命令的服务方法不存在");
                this.RaiseException(packet, exception);
                return;
            }

            // 如果是Cmd值对应是Self类型方法 也就是客户端主动调用服务方法
            if (method.ServiceAttribute.Implement == Implements.Self)
            {
                this.InvokeService(method, packet);
                return;
            }

            // 如果是收到返回值 从回调表找出相关回调来调用
            var callBack = CallbackTable.Take(packet.HashCode);
            if (callBack != null)
            {
                callBack(packet.IsException, packet.GetBodyParameter().FirstOrDefault());
            }
        }


        /// <summary>
        /// 调用服务方法
        /// </summary>    
        /// <param name="method">方法</param>
        /// <param name="packet">数据</param>
        private void InvokeService(ServiceMethod method, FastPacket packet)
        {
            try
            {
                var items = packet.GetBodyParameter();
                var parameters = new object[items.Count];
                for (var i = 0; i < items.Count; i++)
                {
                    parameters[i] = this.Serializer.Deserialize(items[i], method.ParameterTypes[i]);
                }

                var returnValue = method.Invoke(this, parameters);
                if (method.HasReturn && this.IsConnected)
                {
                    packet.SetBodyBinary(this.Serializer, returnValue);
                    this.Send(packet);
                }
            }
            catch (Exception ex)
            {
                this.RaiseException(packet, ex);
            }
        }


        /// <summary>
        /// 当操作中遇到处理异常时，将触发此方法
        /// 并将结果返回给客户端
        /// </summary>       
        /// <param name="packet">数据包</param>
        /// <param name="exception">异常</param>         
        private void RaiseException(FastPacket packet, Exception exception)
        {
            var remoteException = new RemoteException(packet.Command, exception.ToString());
            packet.SetException(this.Serializer, remoteException);
            this.Send(packet);
            this.OnException(exception);
        }

        /// <summary>
        /// 当操作中遇到处理异常时，将触发此方法
        /// </summary>      
        /// <param name="exception">异常</param>
        protected virtual void OnException(Exception exception)
        {
        }

        /// <summary>
        /// 将数据发送到远程端        
        /// </summary>       
        /// <param name="cmd">数据包的Action值</param>
        /// <param name="parameters">参数列表</param>
        protected void InvokeRemote(int cmd, params object[] parameters)
        {
            var packet = new FastPacket(cmd);
            packet.SetBodyBinary(this.Serializer, parameters);
            this.Send(packet);
        }

        /// <summary>
        /// 将数据发送到远程端     
        /// 并返回结果数据任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="cmd">数据包的命令值</param>
        /// <param name="parameters"></param>
        /// <returns>参数列表</returns>
        /// <exception cref="RemoteException"></exception>
        protected Task<T> InvokeRemote<T>(int cmd, params object[] parameters)
        {
            var taskSource = new TaskCompletionSource<T>();
            var packet = new FastPacket(cmd);
            packet.SetBodyBinary(this.Serializer, parameters);

            // 发送之前记录回参数
            Action<bool, byte[]> callBack = (isException, bytes) =>
            {
                if (isException == false)
                {
                    var result = (T)this.Serializer.Deserialize(bytes, typeof(T));
                    taskSource.SetResult(result);
                }
                else
                {
                    var exception = (RemoteException)this.Serializer.Deserialize(bytes, typeof(RemoteException));
                    if (exception != null)
                    {
                        taskSource.TrySetException(exception);
                    }
                }
            };
            CallbackTable.Add(packet.HashCode, callBack);

            this.Send(packet);
            return taskSource.Task;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否也释放托管资源</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                this.serverMethods.Clear();
                this.serverMethods = null;
                this.Serializer = null;
            }
        }
    }
}
