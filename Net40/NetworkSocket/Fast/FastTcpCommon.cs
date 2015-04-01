using NetworkSocket.Fast.Attributes;
using NetworkSocket.Fast.Methods;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// 获取类型的服务方法
        /// </summary>
        /// <param name="seviceType">类型</param>
        /// <returns></returns>
        public static List<ServiceMethod> GetServiceMethodList(Type seviceType)
        {
            var methods = seviceType.GetMethods().Where(item => Attribute.IsDefined(item, typeof(ServiceAttribute)));
            return methods.Select(item => new ServiceMethod(item)).ToList();
        }

        /// <summary>
        /// 抛出远程异常
        /// 如果无法抛出，则返回远程异常
        /// </summary>       
        /// <param name="packet">异常包</param>
        /// <param name="serializer">序列化工具</param>
        /// <returns></returns>
        public static RemoteException ThrowRemoteException(FastPacket packet, ISerializer serializer)
        {
            if (packet.IsException == false)
            {
                return null;
            }

            var bytes = packet.GetBodyParameter().FirstOrDefault();
            var exceptionCallBack = CallbackTable.Take(packet.HashCode);

            if (exceptionCallBack != null)
            {
                exceptionCallBack(packet.IsException, bytes);
                return null;
            }
            return serializer.Deserialize(bytes, typeof(RemoteException)) as RemoteException;
        }

        /// <summary>       
        /// 触发远程异常
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="packet">封包</param>
        /// <param name="exception">异常</param>     
        /// <param name="serializer">序列化工具</param>
        public static void RaiseRemoteException(SocketAsync<FastPacket> client, FastPacket packet, Exception exception, ISerializer serializer)
        {
            var remoteException = new RemoteException(packet.Command, exception.ToString());
            packet.SetException(serializer, remoteException);
            client.Send(packet);
        }

        /// <summary>
        /// 将数据发送到远程端        
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="serializer">序列化工具</param>
        /// <param name="cmd">数据包的Action值</param>
        /// <param name="parameters">参数列表</param>
        /// <exception cref="RemoteException"></exception>
        public static void InvokeRemote(SocketAsync<FastPacket> client, ISerializer serializer, int cmd, params object[] parameters)
        {
            var packet = new FastPacket(cmd);
            packet.SetBodyBinary(serializer, parameters);
            client.Send(packet);
        }

        /// <summary>
        /// 将数据发送到远程端     
        /// 并返回结果数据任务
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="client">客户端</param>
        /// <param name="serializer">序列化工具</param>
        /// <param name="cmd">数据包的命令值</param>
        /// <param name="parameters"></param>
        /// <returns>参数列表</returns>
        public static Task<T> InvokeRemote<T>(SocketAsync<FastPacket> client, ISerializer serializer, int cmd, params object[] parameters)
        {
            var taskSource = new TaskCompletionSource<T>();
            var packet = new FastPacket(cmd);
            packet.SetBodyBinary(serializer, parameters);

            // 发送之前记录回参数
            Action<bool, byte[]> callBack = (isException, bytes) =>
            {
                if (isException == false)
                {
                    var result = (T)serializer.Deserialize(bytes, typeof(T));
                    taskSource.SetResult(result);
                }
                else
                {
                    var exception = serializer.Deserialize(bytes, typeof(RemoteException)) as RemoteException;
                    if (exception != null)
                    {
                        taskSource.TrySetException(exception);
                    }
                }
            };

            CallbackTable.Add(packet.HashCode, callBack);
            client.Send(packet);
            return taskSource.Task;
        }

        /// <summary>
        /// 生成服务方法的调用参数
        /// </summary>        
        /// <param name="method">方法</param>
        /// <param name="packet">数据包</param>
        /// <param name="serializer">序列化工具</param>
        /// <returns></returns>
        public static object[] GetServiceMethodParameters(ServiceMethod method, FastPacket packet, ISerializer serializer)
        {
            var items = packet.GetBodyParameter();
            var parameters = new object[items.Count];
            for (var i = 0; i < items.Count; i++)
            {
                parameters[i] = serializer.Deserialize(items[i], method.ParameterTypes[i]);
            }
            return parameters;
        }


        /// <summary>
        /// 生成服务方法的调用参数
        /// </summary>       
        /// <param name="method">方法</param>       
        /// <param name="packet">数据</param>
        /// <param name="serializer">序列化工具</param>
        /// <param name="client">客户端参数</param>
        public static object[] GetServiceMethodParameters(ServiceMethod method, FastPacket packet, ISerializer serializer, SocketAsync<FastPacket> client)
        {
            var items = packet.GetBodyParameter();
            var parameters = new object[items.Count + 1];
            parameters[0] = client;
            for (var i = 0; i < items.Count; i++)
            {
                parameters[i + 1] = serializer.Deserialize(items[i], method.ParameterTypes[i + 1]);
            }
            return parameters;
        }
    }
}
