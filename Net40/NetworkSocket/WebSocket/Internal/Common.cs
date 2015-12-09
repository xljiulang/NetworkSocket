using NetworkSocket.Core;
using NetworkSocket.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.WebSocket
{
    /// <summary>
    /// JsonWebSocket公共类
    /// </summary>
    internal static class Common
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
            var taskSetAction = taskSetActionTable.Take(requestContext.Packet.id);
            if (taskSetAction != null)
            {
                var returnValue = requestContext.Packet.body;
                var serializer = requestContext.Session.Server.JsonSerializer;
                taskSetAction.SetAction(SetTypes.SetReturnReult, returnValue, serializer);
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
            var body = requestContext.Packet.body;
            var message = body == null ? null : body.ToString();
            var taskSetAction = taskSetActionTable.Take(requestContext.Packet.id);
            if (taskSetAction != null)
            {
                taskSetAction.SetAction(SetTypes.SetReturnException, message, null);
                return null;
            }
            return new RemoteException(message);
        }

        /// <summary>       
        /// 设置远程异常
        /// </summary>
        /// <param name="serializer">序列化工具</param>
        /// <param name="exceptionContext">上下文</param>       
        /// <returns></returns>
        public static bool SetRemoteException(IDynamicJsonSerializer serializer, ExceptionContext exceptionContext)
        {
            try
            {
                var packet = exceptionContext.Packet;
                packet.state = false;
                packet.body = exceptionContext.Exception.Message;

                var packetJson = serializer.Serialize(packet);
                exceptionContext.Session.SendText(packetJson);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 获取和更新Api行为的参数值
        /// </summary> 
        /// <param name="context">上下文</param>        
        /// <exception cref="ArgumentException"></exception>    
        /// <returns></returns>
        public static object[] GetAndUpdateParameterValues(ActionContext context)
        {
            var body = context.Packet.body as IList;
            if (body == null)
            {
                throw new ArgumentException("body参数必须为数组");
            }

            if (body.Count != context.Action.ParameterTypes.Length)
            {
                throw new ArgumentException("body参数数量不正确");
            }

            var parameters = new object[body.Count];
            var serializer = context.Session.Server.JsonSerializer;

            for (var i = 0; i < body.Count; i++)
            {
                var bodyParameter = body[i];
                var parameterType = context.Action.ParameterTypes[i];
                parameters[i] = serializer.Convert(bodyParameter, parameterType);
            }
            context.Action.ParameterValues = parameters;
            return parameters;
        }
    }
}
