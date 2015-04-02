using NetworkSocket.Fast.Attributes;
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
        /// 所有服务行为
        /// </summary>
        private List<FastAction> fastActionList;

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
            this.fastActionList = FastTcpCommon.GetServiceActions(this.GetType());
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
            if (packet.IsException == false)
            {
                this.ProcessNormalPacket(packet);
                return;
            }

            // 抛出远程异常
            var exception = FastTcpCommon.ThrowRemoteException(packet, this.Serializer);
            if (exception != null)
            {
                var exContext = new ExceptionContext { Client = this, Packet = packet, Exception = exception };
                this.OnException(exContext);
            }
        }

        /// <summary>
        /// 处理正常数据包
        /// </summary>      
        /// <param name="packet">数据包</param>
        private void ProcessNormalPacket(FastPacket packet)
        {
            var requestContext = new RequestContext { Client = this, Packet = packet };
            var action = this.fastActionList.FirstOrDefault(item => item.Command == packet.Command);

            if (action == null)
            {
                var exception = new Exception(string.Format("命令为{0}的服务行为不存在", packet.Command));
                this.RaiseException(new ExceptionContext(requestContext, exception));
                return;
            }

            // 如果是Cmd值对应是Self类型方法 也就是客户端主动调用服务行为
            if (action.Implement == Implements.Self)
            {
                this.TryInvokeAction(new ActionContext(requestContext, action));
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
        /// 将返回值发送给服务器
        /// 或将异常发送给服务器
        /// </summary>    
        /// <param name="actionContext">上下文</param>       
        private void TryInvokeAction(ActionContext actionContext)
        {
            try
            {
                var parameters = FastTcpCommon.GetActionParameters(actionContext, this.Serializer);
                var returnValue = actionContext.Action.Execute(this, parameters);
                if (actionContext.Action.IsVoidReturn == false && this.IsConnected)
                {
                    actionContext.Packet.SetBodyBinary(this.Serializer, returnValue);
                    this.Send(actionContext.Packet);
                }
            }
            catch (Exception ex)
            {
                var exceptionContext = new ExceptionContext(actionContext, ex);
                this.RaiseException(exceptionContext);
            }
        }


        /// <summary>        
        /// 并将异常传给客户端并调用OnException
        /// </summary>       
        /// <param name="exceptionContext">上下文</param>               
        private void RaiseException(ExceptionContext exceptionContext)
        {
            FastTcpCommon.RaiseRemoteException(exceptionContext, this.Serializer);
            this.OnException(exceptionContext);
        }

        /// <summary>
        /// 当操作中遇到处理异常时，将触发此方法
        /// </summary>      
        /// <param name="filterContext">上下文</param>
        protected virtual void OnException(ExceptionContext filterContext)
        {
        }

        /// <summary>
        /// 将数据发送到远程端        
        /// </summary>       
        /// <param name="cmd">数据包的Action值</param>
        /// <param name="parameters">参数列表</param>
        protected void InvokeRemote(int cmd, params object[] parameters)
        {
            FastTcpCommon.InvokeRemote(this, this.Serializer, cmd, parameters);
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
            return FastTcpCommon.InvokeRemote<T>(this, this.Serializer, cmd, parameters);
        }

        /// <summary>
        /// 获取服务组件版本号
        /// </summary>       
        /// <returns></returns>
        [Service(Implements.Remote, (int)SpecialCommands.Version)]
        public Task<string> GetVersion()
        {
            return this.InvokeRemote<string>((int)SpecialCommands.Version);
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
                this.fastActionList.Clear();
                this.fastActionList = null;
                this.Serializer = null;
            }
        }
    }
}
