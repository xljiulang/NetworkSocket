using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 表示Api行为列表
    /// </summary>
    internal class ApiActionList
    {
        /// <summary>
        /// Api行为字典
        /// </summary>
        private Dictionary<string, ApiAction> dictionary;

        /// <summary>
        /// Api行为列表
        /// </summary>
        public ApiActionList()
        {
            this.dictionary = new Dictionary<string, ApiAction>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Api行为列表
        /// </summary>
        /// <param name="apiActions">Api行为</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ApiActionList(IEnumerable<ApiAction> apiActions)
            : this()
        {
            foreach (var action in apiActions)
            {
                this.Add(action);
            }
        }

        /// <summary>
        /// 添加Api行为
        /// </summary>
        /// <param name="apiAction">Api行为</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void Add(ApiAction apiAction)
        {
            if (apiAction == null)
            {
                throw new ArgumentNullException("apiAction");
            }

            if (this.dictionary.ContainsKey(apiAction.ApiName))
            {
                throw new ArgumentException(string.Format("Api行为{0}已存在", apiAction.ApiName));
            }

            this.CheckSelfParameterType(apiAction);
            this.dictionary.Add(apiAction.ApiName, apiAction);
        }

        /// <summary>
        /// 检测参数类型
        /// </summary>
        /// <param name="apiAction">Api行为</param>
        /// <exception cref="ArgumentException"></exception>
        private void CheckSelfParameterType(ApiAction apiAction)
        {
            foreach (var pType in apiAction.ParameterTypes)
            {
                if (pType.IsAbstract || pType.IsInterface)
                {
                    throw new ArgumentException(string.Format("Api{0}的参数类型不能包含抽象类或接口", apiAction.ApiName));
                }

                if (pType.IsSerializable == false)
                {
                    throw new ArgumentException(string.Format("Api{0}的参数类型必须为可序列化", apiAction.ApiName));
                }
            }
        }

        /// <summary>
        /// 添加Api行为
        /// </summary>
        /// <param name="apiActions">Api行为</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void AddRange(IEnumerable<ApiAction> apiActions)
        {
            foreach (var action in apiActions)
            {
                this.Add(action);
            }
        }

        /// <summary>
        /// 获取Api行为
        /// 如果获取不到则返回null
        /// </summary>
        /// <param name="name">行为名称</param>
        /// <returns></returns>
        public ApiAction TryGet(string name)
        {
            ApiAction apiAction;
            if (this.dictionary.TryGetValue(name, out apiAction))
            {
                return apiAction;
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
