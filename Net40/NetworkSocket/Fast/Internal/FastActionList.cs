using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 表示服务行为列表
    /// </summary>
    internal class FastActionList : IEnumerable<FastAction>
    {
        /// <summary>
        /// 服务行为字典
        /// </summary>
        private Dictionary<int, FastAction> dictionary;

        /// <summary>
        /// 服务行为列表
        /// </summary>
        public FastActionList()
        {
            this.dictionary = new Dictionary<int, FastAction>();
        }

        /// <summary>
        /// 服务行为列表
        /// </summary>
        /// <param name="fastActions">服务行为</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public FastActionList(IEnumerable<FastAction> fastActions)
            : this()
        {
            foreach (var action in fastActions)
            {
                this.Add(action);
            }
        }

        /// <summary>
        /// 添加服务行为
        /// </summary>
        /// <param name="fastAction">服务行为</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void Add(FastAction fastAction)
        {
            if (fastAction == null)
            {
                throw new ArgumentNullException("fastAction");
            }

            if (this.dictionary.ContainsKey(fastAction.Command))
            {
                throw new ArgumentException(string.Format("服务行为{0}或其命令值已存在", fastAction.Name));
            }

            if (fastAction.Implement == Implements.Remote)
            {
                this.CheckRemoteReturnType(fastAction);
            }
            else
            {
                this.CheckSelfParameterType(fastAction);
            }

            this.dictionary.Add(fastAction.Command, fastAction);
        }

        /// <summary>
        /// 检测返回类型
        /// </summary>
        /// <param name="fastAction">服务行为</param>
        /// <exception cref="ArgumentException"></exception>
        private void CheckRemoteReturnType(FastAction fastAction)
        {
            var isTask = fastAction.ReturnType.IsGenericType && fastAction.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);
            if ((fastAction.IsVoidReturn || isTask) == false)
            {
                throw new ArgumentException(string.Format("服务行为{0}的返回类型必须是Task<T>类型", fastAction.Name));
            }
        }

        /// <summary>
        /// 检测参数类型
        /// </summary>
        /// <param name="fastAction">服务行为</param>
        /// <exception cref="ArgumentException"></exception>
        private void CheckSelfParameterType(FastAction fastAction)
        {
            foreach (var pType in fastAction.ParameterTypes)
            {
                if (pType.IsAbstract || pType.IsInterface)
                {
                    throw new ArgumentException(string.Format("服务行为{0}的参数类型不能包含抽象类或接口", fastAction.Name));
                }
            }
        }

        /// <summary>
        /// 添加服务行为
        /// </summary>
        /// <param name="fastActions">服务行为</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void AddRange(IEnumerable<FastAction> fastActions)
        {
            foreach (var action in fastActions)
            {
                this.Add(action);
            }
        }

        /// <summary>
        /// 获取服务行为
        /// 如果获取不到则返回null
        /// </summary>
        /// <param name="command">行为命令</param>
        /// <returns></returns>
        public FastAction TryGet(int command)
        {
            FastAction fastAction;
            if (this.dictionary.TryGetValue(command, out fastAction))
            {
                return fastAction;
            }
            return null;
        }

        /// <summary>
        /// 获取是否存在
        /// </summary>
        /// <param name="command">行为命令</param>
        /// <returns></returns>
        public bool IsExist(int command)
        {
            return this.dictionary.ContainsKey(command);
        }

        #region IEnumerable
        public IEnumerator<FastAction> GetEnumerator()
        {
            return this.dictionary.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion
    }
}
