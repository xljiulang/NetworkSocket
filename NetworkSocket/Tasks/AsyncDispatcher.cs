using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkSocket.Tasks
{
    /// <summary>
    /// 提供Async方法异步等待完成
    /// </summary>
    public sealed class AsyncDispatcher : IDisposable
    {
        /// <summary>
        /// 获取当前线程关联的AsyncDispatcher
        /// </summary>
        public static AsyncDispatcher Current
        {
            get
            {
                var context = SynchronizationContext.Current as AsyncSynchronizationContext;
                return context == null ? null : context.Dispatcher;
            }
        }

        /// <summary>
        /// 等待Async方法执行完成
        /// 创建新的同步上下文关联asyncAction
        /// 执行完成后切换为调用线程的同步上下文
        /// </summary>
        /// <param name="asyncAction">Async方法的委托</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public static bool AwaitAsync(Action asyncAction)
        {
            var dispatcher = AsyncDispatcher.Current;
            if (dispatcher != null)
            {
                return dispatcher.Await(asyncAction);
            }

            using (dispatcher = new AsyncDispatcher())
            {
                return dispatcher.Await(asyncAction);
            }
        }

        /// <summary>
        /// 队列
        /// </summary>
        private readonly SyncCallbackQueue callbackQuque = new SyncCallbackQueue();

        /// <summary>
        /// 等待Async方法执行完成
        /// 创建新的同步上下文关联asyncAction
        /// 执行完成后切换为调用线程的同步上下文
        /// </summary>
        /// <param name="asyncAction">Async方法的委托</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public bool Await(Action asyncAction)
        {
            if (asyncAction == null)
            {
                throw new ArgumentNullException();
            }

            var previousContext = SynchronizationContext.Current;
            try
            {
                var currentContext = new AsyncSynchronizationContext(this);
                SynchronizationContext.SetSynchronizationContext(currentContext);
                asyncAction.Invoke();
                return currentContext.WaitForPendingOperationsToComplete() > 0L;
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.callbackQuque.Dispose();
        }



        /// <summary>
        /// 表示同步上下文委托队列
        /// </summary>
        private class SyncCallbackQueue : IDisposable
        {
            /// <summary>
            /// 是否在运行中
            /// </summary>
            private bool running = true;

            /// <summary>
            /// 阻塞/通知事件
            /// </summary>
            private readonly AutoResetEvent resetEvent = new AutoResetEvent(false);

            /// <summary>
            /// 回调队列
            /// </summary>
            private readonly ConcurrentQueue<SyncCallback> quque = new ConcurrentQueue<SyncCallback>();

            /// <summary>
            /// 添加到队列中
            /// </summary>
            /// <param name="d">委托</param>
            /// <param name="state">参数</param>
            public void Enqueue(SendOrPostCallback d, object state)
            {
                var callBack = new SyncCallback(d, state);
                this.quque.Enqueue(callBack);
                this.resetEvent.Set();
            }

            /// <summary>
            /// 标记为已完成
            /// </summary>
            public void MarkAsComplete()
            {
                this.running = false;
                this.resetEvent.Set();
            }

            /// <summary>
            /// 执行所有待执行的回调
            /// </summary>
            public void InvokeAll()
            {
                while (this.running)
                {
                    this.InvokePendingCallback();
                    this.resetEvent.WaitOne();
                }
                this.InvokePendingCallback();
            }

            /// <summary>
            /// 执行所有待执行的回调
            /// </summary>
            private void InvokePendingCallback()
            {
                SyncCallback callback;
                while (this.quque.TryDequeue(out callback))
                {
                    callback.Invoke();
                }
            }

            /// <summary>
            /// 释放资源
            /// </summary>
            public void Dispose()
            {
                this.resetEvent.Dispose();
            }


            /// <summary>
            /// 在同步上下文执行的委托
            /// </summary>
            private class SyncCallback
            {
                /// <summary>
                /// 参数
                /// </summary>
                private readonly object state;
                /// <summary>
                /// 委托
                /// </summary>
                private readonly SendOrPostCallback callback;

                /// <summary>
                /// 在同步上下文执行的委托
                /// </summary>
                /// <param name="callback">委托</param>
                /// <param name="state">参数</param>
                public SyncCallback(SendOrPostCallback callback, object state)
                {
                    this.callback = callback;
                    this.state = state;
                }

                /// <summary>
                /// 在同步上下文执行
                /// </summary>
                public void Invoke()
                {
                    this.callback.Invoke(state);
                }
            }
        }


        /// <summary>
        /// 表示Async的同步上下文
        /// </summary>
        private class AsyncSynchronizationContext : SynchronizationContext
        {
            /// <summary>
            /// 当前任务数
            /// </summary>
            private long taskCount = 0L;

            /// <summary>
            /// 获取调度器
            /// </summary>
            public AsyncDispatcher Dispatcher { get; private set; }

            /// <summary>
            /// Async的同步上下文
            /// </summary>
            /// <param name="dispatcher">调度器</param>
            public AsyncSynchronizationContext(AsyncDispatcher dispatcher)
            {
                this.Dispatcher = dispatcher;
            }

            /// <summary>
            /// 复制副本
            /// </summary>
            /// <returns></returns>
            public override SynchronizationContext CreateCopy()
            {
                return new AsyncSynchronizationContext(this.Dispatcher);
            }

            /// <summary>
            /// Post到同步上下文
            /// </summary>
            /// <param name="d"></param>
            /// <param name="state"></param>
            public override void Post(SendOrPostCallback d, object state)
            {
                this.Dispatcher.callbackQuque.Enqueue(d, state);
            }

            /// <summary>
            /// 操作开始
            /// </summary>
            public override void OperationStarted()
            {
                Interlocked.Increment(ref this.taskCount);
                base.OperationStarted();
            }

            /// <summary>
            /// 操作结束
            /// </summary>
            public override void OperationCompleted()
            {
                if (Interlocked.Decrement(ref this.taskCount) == 0L)
                {
                    this.Dispatcher.callbackQuque.MarkAsComplete();
                }
                base.OperationCompleted();
            }


            /// <summary>
            /// 等待未完成的任务
            /// </summary>
            public long WaitForPendingOperationsToComplete()
            {
                var count = Interlocked.Read(ref this.taskCount);
                if (count > 0L)
                {
                    this.Dispatcher.callbackQuque.InvokeAll();
                }
                return count;
            }
        }

    }
}