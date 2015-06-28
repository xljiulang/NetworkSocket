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
        public TaskSetAction(ISerializer serializer, TaskCompletionSource<T> taskSource)
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
            switch (setType)
            {
                case SetTypes.SetReturnReult:
                    this.SetResult(bytes);
                    break;

                case SetTypes.SetReturnException:
                    var message = bytes == null ? string.Empty : Encoding.UTF8.GetString(bytes);
                    var remoteException = new RemoteException(message);
                    this.taskSource.TrySetException(remoteException);
                    break;

                case SetTypes.SetTimeoutException:
                    var timeoutException = new TimeoutException();
                    this.taskSource.TrySetException(timeoutException);
                    break;

                case SetTypes.SetShutdownException:
                    var shutdownException = new SocketException(SocketError.Shutdown.GetHashCode());
                    this.taskSource.TrySetException(shutdownException);
                    break;
            }
        }

        /// <summary>
        /// 设置结果
        /// </summary>
        /// <param name="bytes">数据</param>
        private void SetResult(byte[] bytes)
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
    }
}
