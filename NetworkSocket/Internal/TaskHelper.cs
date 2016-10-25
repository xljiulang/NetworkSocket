using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket
{
    /// <summary>
    /// 任务helper
    /// </summary>
    internal static class TaskHelper
    {
        /// <summary>
        /// 表示已完成的任务
        /// </summary>
        public static readonly Task Completed = Task.FromResult(true);

        /// <summary>
        /// 安全字典
        /// </summary>
        private readonly static ConcurrentDictionary<Type, Func<Task, object>> cache = new ConcurrentDictionary<Type, Func<Task, object>>();


        /// <summary>
        /// 从value值转换得到
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="valueType">值类型</param>
        /// <returns></returns>
        public async static Task<object> CastTaskFrom(object value, Type valueType)
        {
            var task = value as Task;
            if (task == null)
            {
                return value;
            }
            else
            {
                await task;
                return TaskHelper.cache
                    .GetOrAdd(valueType, (type) => TaskHelper.CreateTaskResultInvoker(type))
                    .Invoke(task);
            }
        }

        /// <summary>
        /// 创建Task类型获取Result的委托
        /// </summary>
        /// <param name="taskType">Task实例的类型</param>
        /// <returns></returns>
        private static Func<Task, object> CreateTaskResultInvoker(Type taskType)
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
