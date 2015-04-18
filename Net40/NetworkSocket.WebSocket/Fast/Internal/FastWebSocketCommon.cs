using Newtonsoft.Json.Linq;
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
                var returnJson = ((object)requestContext.Packet.body).ToString();
                if (returnJson == "True" || returnJson == "False")
                {
                    returnJson = returnJson.ToLower();
                }
                taskSetAction.SetAction(SetTypes.SetReturnReult, returnJson);
            }
        }


        /// <summary>
        /// 设置Api行为返回的任务异常 
        /// 设置失败则返远程异常对象
        /// </summary>          
        /// <param name="serializer">序列化工具</param>
        /// <param name="taskSetActionTable">任务行为表</param>
        /// <param name="requestContext">请求上下文</param>
        /// <returns></returns>
        public static RemoteException SetApiActionTaskException(IJsonSerializer serializer, TaskSetActionTable taskSetActionTable, RequestContext requestContext)
        {
            var message = ((object)requestContext.Packet.body).ToString();
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
            var packet = exceptionContext.Packet;
            packet.state = false;
            packet.body = exceptionContext.Exception.Message;

            var json = serializer.Serialize(packet);
            return exceptionContext.Client.TrySend(json);
        }

        /// <summary>
        /// 生成Api行为的调用参数
        /// </summary>        
        /// <param name="serializer">序列化工具</param>
        /// <param name="context">上下文</param> 
        /// <returns></returns>
        public static object[] GetApiActionParameters(IJsonSerializer serializer, ActionContext context)
        {
            if (context.Packet.body == null)
            {
                return new object[0];
            }

            var index = 0;
            var parameters = new object[context.Action.ParameterTypes.Length];

            foreach (object bodyParameter in context.Packet.body)
            {
                var parameterType = context.Action.ParameterTypes[index];
                var parameterJson = bodyParameter.ToString();

                if (parameterJson == "True" || parameterJson == "Flase")
                {
                    parameterJson = parameterJson.ToLower();
                }

                if (parameterJson == null || parameterJson.Length == 0)
                {
                    parameters[index] = Activator.CreateInstance(parameterType);
                }
                else
                {
                    parameters[index] = serializer.Deserialize(parameterJson, parameterType);
                }
                index++;
            }
            return parameters;
        }
    }
}
