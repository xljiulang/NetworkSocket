using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 任务设置行为接口
    /// </summary>
    internal interface ITaskSetAction
    {
        /// <summary>
        /// 获取创建时间
        /// </summary>
        int CreateTime { get; }

        /// <summary>
        /// 设置行为
        /// </summary>
        /// <param name="setType">行为类型</param>
        /// <param name="bytes">数据值</param>
        void SetAction(SetTypes setType, byte[] bytes);
    }

    /// <summary>
    /// 任务设置行为信息
    /// </summary>
    [DebuggerDisplay("InitTime = {InitTime}")]
    internal class TaskSetAction<T> : ITaskSetAction
    {
        /// <summary>
        /// 序列化工具
        /// </summary>
        private ISerializer serializer;

        /// <summary>
        /// 任务源
        /// </summary>
        private TaskCompletionSource<T> taskSource;

        /// <summary>
        /// 获取创建时间
        /// </summary>
        public int CreateTime { get; private set; }        

        /// <summary>
        /// 任务设置行为
        /// </summary>       
        /// <param name="serializer">序列化工具</param>
        /// <param name="taskSource">任务源</param>
        public TaskSetAction( ISerializer serializer, TaskCompletionSource<T> taskSource)
        {           
            this.serializer = serializer;
            this.taskSource = taskSource;
            this.CreateTime = Environment.TickCount;
        }

        /// <summary>
        /// 设置行为
        /// </summary>
        /// <param name="setType">行为类型</param>
        /// <param name="bytes">数据值</param>
        public void SetAction(SetTypes setType, byte[] bytes)
        {
            if (setType == SetTypes.SetReturnReult)
            {
                if (bytes == null || bytes.Length == 0)
                {
                    this.taskSource.TrySetResult(default(T));
                    return;
                }

                try
                {
                    var result = (T)this.serializer.Deserialize(bytes, typeof(T));
                    this.taskSource.TrySetResult(result);
                }
                catch (SerializerException ex)
                {
                    this.taskSource.TrySetException(ex);
                }
                catch (Exception ex)
                {
                    this.taskSource.TrySetException(new SerializerException(ex));
                }
            }
            else if (setType == SetTypes.SetReturnException)
            {
                var message = bytes == null ? string.Empty : Encoding.UTF8.GetString(bytes);
                var exception = new RemoteException(message);
                this.taskSource.TrySetException(exception);
            }
            else if (setType == SetTypes.SetTimeoutException)
            {
                var exception = new TimeoutException();
                this.taskSource.TrySetException(exception);
            }
            else if (setType == SetTypes.SetShutdownException)
            {
                var exception = new SocketException(SocketError.Shutdown.GetHashCode());
                this.taskSource.TrySetException(exception);
            }
        }
    }
}
