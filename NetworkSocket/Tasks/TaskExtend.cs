using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Tasks
{
    /// <summary>
    /// 提供Task的扩展
    /// </summary>
    public static class TaskExtend
    {
        /// <summary>
        /// 安全字典
        /// </summary>
        private readonly static ConcurrentDictionary<Type, Func<Task, object>> cache = new ConcurrentDictionary<Type, Func<Task, object>>();

        /// <summary>
        /// 表示已完成的task
        /// </summary>
        public static readonly Task CompletedTask = Task.FromResult(true);

        /// <summary>
        /// 转换为TaskOf(T)类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task">任务</param>
        /// <returns></returns>
        public static Task<T> ToTask<T>(this Task task)
        {
            return task.ToTask<T>(task.GetType());
        }

        /// <summary>
        /// 转换为TaskOf(T)类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task">任务</param>
        /// <param name="taskType">任务类型</param>
        /// <returns></returns>
        public async static Task<T> ToTask<T>(this Task task, Type taskType)
        {
            await task;
            return (T)TaskExtend.cache
                .GetOrAdd(taskType, (type) => TaskExtend.CreateResultInvoker(type))
                .Invoke(task);
        }

        /// <summary>
        /// 创建Task类型获取Result的委托
        /// </summary>
        /// <param name="taskType">Task实例的类型</param>
        /// <returns></returns>
        private static Func<Task, object> CreateResultInvoker(Type taskType)
        {
            if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                // task => (object)(((Task<T>)task).Result)
                var arg = Expression.Parameter(typeof(Task));
                var castArg = Expression.Convert(arg, taskType);
                var fieldAccess = Expression.Property(castArg, "Result");
                var castResult = Expression.Convert(fieldAccess, typeof(object));
                var lambda = Expression.Lambda<Func<Task, object>>(castResult, arg);
                return lambda.Compile();
            }
            else
            {
                return task => null;
            }
        }
    }
}
