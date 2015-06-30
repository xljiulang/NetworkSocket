using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.WebSocket.Fast
{
    /// <summary>
    /// JsonWebSocket公共类
    /// </summary>
    internal static class FastWebSocketCommon
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
                taskSetAction.SetAction(SetTypes.SetReturnReult, returnValue);
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
            var message = JObject.TryCast<string>(requestContext.Packet.body);
            var taskSetAction = taskSetActionTable.Take(requestContext.Packet.id);

            if (taskSetAction != null)
            {
                taskSetAction.SetAction(SetTypes.SetReturnException, message);
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
        public static bool SetRemoteException(IJsonSerializer serializer, ExceptionContext exceptionContext)
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
        /// 生成Api行为的调用参数
        /// </summary> 
        /// <param name="context">上下文</param>        
        /// <exception cref="SerializerException"></exception>
        /// <returns></returns>
        public static object[] GetApiActionParameters(ActionContext context)
        {
            var body = context.Packet.body as JObject;
            if (body == null || body.IsArray == false)
            {
                throw new ArgumentException("body参数必须为数组");
            }

            if (body.Length != context.Action.ParameterTypes.Length)
            {
                throw new ArgumentException("body参数数量不正确");
            }

            var parameters = new object[body.Length];
            for (var i = 0; i < body.Length; i++)
            {
                var bodyParameter = body[i];
                var parameterType = context.Action.ParameterTypes[i];
                parameters[i] = JObject.Cast(bodyParameter, parameterType);
            }
            return parameters;
        }
    }
}
