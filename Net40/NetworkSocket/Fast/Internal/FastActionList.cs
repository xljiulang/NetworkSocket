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
    internal class FastActionList
    {
        /// <summary>
        /// 服务行为字典
        /// </summary>
        private Dictionary<string, FastAction> dictionary;

        /// <summary>
        /// 服务行为列表
        /// </summary>
        public FastActionList()
        {
            this.dictionary = new Dictionary<string, FastAction>(StringComparer.CurrentCultureIgnoreCase);
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

            if (this.dictionary.ContainsKey(fastAction.ApiName))
            {
                throw new ArgumentException(string.Format("服务行为{0}已存在", fastAction.ApiName));
            }

            this.CheckSelfParameterType(fastAction);
            this.dictionary.Add(fastAction.ApiName, fastAction);
        }

        /// <summary>
        /// 检测参数类型
        /// </summary>
        /// <param name="fastAction">服务行为</param>
        /// <exception cref="ArgumentException"></exception>
        private void CheckSelfParameterType(FastAction fastAction)
        {
            if (fastAction.ReturnType.IsSerializable == false)
            {
                throw new ArgumentException(string.Format("Api{0}的返回类型必须为可序列化", fastAction.ApiName));
            }

            foreach (var pType in fastAction.ParameterTypes)
            {
                if (pType.IsAbstract || pType.IsInterface)
                {
                    throw new ArgumentException(string.Format("Api{0}的参数类型不能包含抽象类或接口", fastAction.ApiName));
                }

                if (pType.IsSerializable == false)
                {
                    throw new ArgumentException(string.Format("Api{0}的参数类型必须为可序列化", fastAction.ApiName));
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
        /// <param name="name">行为名称</param>
        /// <returns></returns>
        public FastAction TryGet(string name)
        {
            FastAction fastAction;
            if (this.dictionary.TryGetValue(name, out fastAction))
            {
                return fastAction;
            }
            return null;
        }

        /// <summary>
        /// 获取是否存在
        /// </summary>
        /// <param name="name">行为名称</param>
        /// <returns></returns>
        public bool IsExist(string name)
        {
            return this.dictionary.ContainsKey(name);
        }
    }
}
