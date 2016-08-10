using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Util
{
    /// <summary>
    /// 提供TaskOf(T)的Result值获取
    /// </summary>
    public static class TaskResult
    {
        /// <summary>
        /// 安全字典
        /// </summary>
        private readonly static ConcurrentDictionary<Type, Func<Task, object>> dic = new ConcurrentDictionary<Type, Func<Task, object>>();

        /// <summary>
        /// 创建Task类型获取Result的委托
        /// </summary>
        /// <param name="taskType">Task实例的类型</param>
        /// <returns></returns>
        private static Func<Task, object> CreateTaskResultInvoker(Type taskType)
        {
            if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                // lambda = arg => (object)(((Task<T>)task).Result)
                var arg = Expression.Parameter(typeof(Task));
                var castArg = Expression.Convert(arg, taskType);
                var fieldAccess = Expression.Property(castArg, "Result");
                var castResult = Expression.Convert(fieldAccess, typeof(object));
                var lambda = Expression.Lambda<Func<Task, object>>(castResult, arg);
                return lambda.Compile();
            }

            return task => null;
        }

        /// <summary>
        /// 获取task的Result值
        /// </summary>
        /// <param name="task">Task实例</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public static object GetResult(Task task)
        {
            if (task == null)
            {
                throw new ArgumentNullException("task");
            }
            var taskType = task.GetType();
            return TaskResult.GetResult(task, taskType);
        }

        /// <summary>
        /// 获取task的Result值
        /// </summary>
        /// <param name="task">Task实例</param>
        /// <param name="taskType">Task实例的类型</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public static object GetResult(Task task, Type taskType)
        {
            if (task == null)
            {
                throw new ArgumentNullException("task");
            }
            if (taskType == null)
            {
                throw new ArgumentNullException("taskType");
            }
            var invoker = TaskResult.dic.GetOrAdd(taskType, (type) => TaskResult.CreateTaskResultInvoker(type));
            return invoker.Invoke(task);
        }
    }
}
