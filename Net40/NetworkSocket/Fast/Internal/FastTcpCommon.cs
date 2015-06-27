using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// FastTcp公共类
    /// </summary>
    internal static class FastTcpCommon
    {
        /// <summary>
        /// 获取服务类型的Api行为
        /// </summary>
        /// <param name="seviceType">服务类型</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public static IEnumerable<ApiAction> GetServiceApiActions(Type seviceType)
        {
            return seviceType
                .GetMethods()
                .Where(item => Attribute.IsDefined(item, typeof(ApiAttribute)))
                .Select(method => new ApiAction(method));
        }

        /// <summary>
        /// 设置Api行为返回的任务结果
        /// </summary>
        /// <param name="requestContext">上下文</param>
        /// <param name="taskSetActionTable">任务行为表</param>
        public static void SetApiActionTaskResult(RequestContext requestContext, TaskSetActionTable taskSetActionTable)
        {
            var taskSetAction = taskSetActionTable.Take(requestContext.Packet.Id);
            if (taskSetAction != null)
            {
                var returnBytes = requestContext.Packet.Body;
                taskSetAction.SetAction(SetTypes.SetReturnReult, returnBytes);
            }
        }


        /// <summary>
        /// 设置Api行为返回的任务异常 
        /// 设置失败则返远程异常对象
        /// </summary>
        /// <param name="taskSetActionTable">任务行为表</param>
        /// <param name="requestContext">请求上下文</param>
        /// <returns></returns>
        public static RemoteException SetApiActionTaskException(TaskSetActionTable taskSetActionTable, RequestContext requestContext)
        {
            var exceptionBytes = requestContext.Packet.Body;
            var taskSetAction = taskSetActionTable.Take(requestContext.Packet.Id);

            if (taskSetAction != null)
            {
                taskSetAction.SetAction(SetTypes.SetReturnException, exceptionBytes);
                return null;
            }

            var message = Encoding.UTF8.GetString(exceptionBytes);
            return new RemoteException(message);
        }

        /// <summary>       
        /// 设置远程异常
        /// </summary>
        /// <param name="session">会话对象</param>       
        /// <param name="exceptionContext">上下文</param>  
        /// <returns></returns>
        public static bool SetRemoteException(ISession session, ExceptionContext exceptionContext)
        {
            var packet = exceptionContext.Packet;
            packet.IsException = true;
            packet.Body = Encoding.UTF8.GetBytes(exceptionContext.Exception.Message);
            try
            {
                session.Send(packet.ToByteRange());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 调用远程端的Api     
        /// 并返回结果数据任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="session">会话对象</param>
        /// <param name="taskSetActionTable">任务行为表</param>
        /// <param name="serializer">序列化工具</param>      
        /// <param name="packet">封包</param>      
        /// <exception cref="SocketException"></exception>   
        public static Task<T> InvokeApi<T>(ISession session, TaskSetActionTable taskSetActionTable, ISerializer serializer, FastPacket packet)
        {
            var taskSource = new TaskCompletionSource<T>();
            var taskSetAction = new TaskSetAction<T>(serializer, taskSource);
            taskSetActionTable.Add(packet.Id, taskSetAction);

            session.Send(packet.ToByteRange());
            return taskSource.Task;
        }
    }
}
