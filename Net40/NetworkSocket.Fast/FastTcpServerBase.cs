using NetworkSocket.Fast.Attributes;
using NetworkSocket.Fast.Methods;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace NetworkSocket.Fast
{
    /// <summary>
    /// 快速构建Tcp服务端抽象类 
    /// </summary>
    public abstract class FastTcpServerBase : TcpServerBase<FastPacket>
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
        public FastTcpServerBase()
        {
            this.Serializer = new DefaultSerializer();
            this.serverMethods = FastTcpCommon.GetServiceMethodList(this.GetType());

            this.CheckServiceMethodRepeatCommand(this.serverMethods);
            foreach (var method in this.serverMethods)
            {
                this.CheckForServiceMethodContract(method);
            }
        }

        /// <summary>
        /// 检测服务方法是否有声明相同的Command
        /// </summary>
        /// <param name="methods"></param>
        private void CheckServiceMethodRepeatCommand(IEnumerable<ServiceMethod> methods)
        {
            var group = methods.GroupBy(item => item.ServiceAttribute.Command).FirstOrDefault(g => g.Count() > 1);
            if (group != null)
            {
                throw new Exception(string.Format("Command为{0}不允许被重复使用", group.Key));
            }
        }

        /// <summary>
        /// 检测服务方法的声明和参数
        /// </summary>
        /// <param name="method">服务方法</param>
        private void CheckForServiceMethodContract(ServiceMethod method)
        {
            if (Enum.IsDefined(typeof(SpecialCommands), method.ServiceAttribute.Command) && method.Method.IsDefined(typeof(SpecialServiceAttribute), false) == false)
            {
                throw new Exception(string.Format("服务方法{0}的Command是不允许使用的SpecialCommand命令", method.Method.Name));
            }

            if (method.ParameterTypes.Length == 0 || method.ParameterTypes.First().Equals(typeof(SocketAsync<FastPacket>)) == false)
            {
                throw new Exception(string.Format("服务方法{0}的第一个参数必须是SocketAsync<FastPacket>类型", method.Method.Name));
            }

            if (method.ServiceAttribute.Implement == Implements.Remote)
            {
                var returnType = method.Method.ReturnType;
                var isTask = returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>);
                var isVoid = method.HasReturn == false;
                if ((isVoid || isTask) == false)
                {
                    throw new Exception(string.Format("服务方法{0}的的返回类型必须是Task<T>类型", method.Method.Name));
                }
            }
        }

        /// <summary>
        /// 当接收到远程端的数据时，将触发此方法
        /// 此方法用于处理和分析收到的数据
        /// 如果得到一个数据包，将触发OnRecvComplete方法
        /// [注]这里只需处理一个数据包的流程
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="recvBuilder">接收到的历史数据</param>
        /// <returns>如果不够一个数据包，则请返回null</returns>
        protected override FastPacket OnReceive(SocketAsync<FastPacket> client, ByteBuilder recvBuilder)
        {
            return FastPacket.GetPacket(recvBuilder);
        }

        /// <summary>
        /// 当接收到客户端数据包时，将触发此方法
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="packet">数据包</param>
        protected override void OnRecvComplete(SocketAsync<FastPacket> client, FastPacket packet)
        {
            if (packet.IsException == false)
            {
                this.ProcessNormalPacket(client, packet);
                return;
            }

            // 抛出远程异常
            var exception = FastTcpCommon.ThrowRemoteException(packet, this.Serializer);
            if (exception != null)
            {
                this.OnException(client, exception);
            }
        }

        /// <summary>
        /// 处理正常数据包
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="packet">数据包</param>
        private void ProcessNormalPacket(SocketAsync<FastPacket> client, FastPacket packet)
        {
            var method = this.serverMethods.Find(item => item.ServiceAttribute.Command == packet.Command);
            if (method == null)
            {
                var exception = new Exception(string.Format("命令为{0}的服务方法不存在", packet.Command));
                this.RaiseException(client, packet, exception);
                return;
            }

            // 如果是Cmd值对应是Self类型方法 也就是客户端主动调用服务方法
            if (method.ServiceAttribute.Implement == Implements.Self)
            {
                this.TryInvoke(method, client, packet);
                return;
            }

            // 如果是收到返回值 从回调表找出相关回调来调用
            var callBack = CallbackTable.Take(packet.HashCode);
            if (callBack != null)
            {
                var returnBytes = packet.GetBodyParameter().FirstOrDefault();
                callBack(packet.IsException, returnBytes);
            }
        }


        /// <summary>
        /// 调用自身方法
        /// 将返回值发送给客户端
        /// 或将异常发送给客户端
        /// </summary>       
        /// <param name="method">方法</param>
        /// <param name="client">客户端对象</param>
        /// <param name="packet">数据</param>
        private void TryInvoke(ServiceMethod method, SocketAsync<FastPacket> client, FastPacket packet)
        {
            try
            {
                // 执行Filter特性
                foreach (var filter in method.Filters)
                {
                    filter.OnExecuting(client, packet);
                }

                var parameters = FastTcpCommon.GetServiceMethodParameters(method, packet, this.Serializer, client);
                var returnValue = method.Invoke(this, parameters);

                foreach (var filter in method.Filters)
                {
                    filter.OnExecuted(client, packet);
                }

                if (method.HasReturn && client.IsConnected)
                {
                    packet.SetBodyBinary(this.Serializer, returnValue);
                    client.Send(packet);
                }
            }
            catch (Exception ex)
            {
                this.RaiseException(client, packet, ex);
            }
        }


        /// <summary>
        /// 并将异常传给客户端并调用OnException
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="packet">封包</param>
        /// <param name="exception">异常</param>         
        private void RaiseException(SocketAsync<FastPacket> client, FastPacket packet, Exception exception)
        {
            FastTcpCommon.RaiseRemoteException(client, packet, exception, this.Serializer);
            this.OnException(client, exception);
        }

        /// <summary>
        /// 当操作中遇到处理异常时，将触发此方法
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="exception">异常</param>
        protected virtual void OnException(SocketAsync<FastPacket> client, Exception exception)
        {
        }

        /// <summary>
        /// 将数据发送到远程端        
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="cmd">数据包的Action值</param>
        /// <param name="parameters">参数列表</param>
        /// <exception cref="RemoteException"></exception>
        protected void InvokeRemote(SocketAsync<FastPacket> client, int cmd, params object[] parameters)
        {
            FastTcpCommon.InvokeRemote(client, this.Serializer, cmd, parameters);
        }

        /// <summary>
        /// 将数据发送到远程端     
        /// 并返回结果数据任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="client">客户端</param>
        /// <param name="cmd">数据包的命令值</param>
        /// <param name="parameters"></param>
        /// <returns>参数列表</returns>
        protected Task<T> InvokeRemote<T>(SocketAsync<FastPacket> client, int cmd, params object[] parameters)
        {
            return FastTcpCommon.InvokeRemote<T>(client, this.Serializer, cmd, parameters);
        }

        /// <summary>
        /// 生成客户端代理代码
        /// </summary>
        /// <param name="client">客户端</param>
        /// <returns></returns>
        [SpecialService]
        [Service(Implements.Self, (int)SpecialCommands.ProxyCode)]
        public string GetProxyCode(SocketAsync<FastPacket> client)
        {
            return new ProxyMaker(this.GetType()).ToString();
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
