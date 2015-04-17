using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSocket.WebSocket.Json
{
    /// <summary>
    /// 表示服务行为列表
    /// </summary>
    internal class JsonActionList
    {
        /// <summary>
        /// 服务行为字典
        /// </summary>
        private Dictionary<string, JsonAction> dictionary;

        /// <summary>
        /// 服务行为列表
        /// </summary>
        public JsonActionList()
        {
            this.dictionary = new Dictionary<string, JsonAction>(StringComparer.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// 服务行为列表
        /// </summary>
        /// <param name="jsonActions">服务行为</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public JsonActionList(IEnumerable<JsonAction> jsonActions)
            : this()
        {
            foreach (var action in jsonActions)
            {
                this.Add(action);
            }
        }

        /// <summary>
        /// 添加服务行为
        /// </summary>
        /// <param name="jsonAction">服务行为</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void Add(JsonAction jsonAction)
        {
            if (jsonAction == null)
            {
                throw new ArgumentNullException("jsonAction");
            }

            if (this.dictionary.ContainsKey(jsonAction.ApiName))
            {
                throw new ArgumentException(string.Format("服务行为{0}或其命令值已存在", jsonAction.ApiName));
            }
            
            this.CheckSelfParameterType(jsonAction);
            this.dictionary.Add(jsonAction.ApiName, jsonAction);
        }

        /// <summary>
        /// 检测参数类型
        /// </summary>
        /// <param name="jsonAction">服务行为</param>
        /// <exception cref="ArgumentException"></exception>
        private void CheckSelfParameterType(JsonAction jsonAction)
        {
            if (jsonAction.ReturnType.IsSerializable == false)
            {
                throw new ArgumentException(string.Format("Api{0}的返回类型必须为可序列化", jsonAction.ApiName));
            }

            foreach (var pType in jsonAction.ParameterTypes)
            {
                if (pType.IsAbstract || pType.IsInterface)
                {
                    throw new ArgumentException(string.Format("Api{0}的参数类型不能包含抽象类或接口", jsonAction.ApiName));
                }

                if (pType.IsSerializable == false)
                {
                    throw new ArgumentException(string.Format("Api{0}的参数类型必须为可序列化", jsonAction.ApiName));
                }
            }
        }

        /// <summary>
        /// 添加服务行为
        /// </summary>
        /// <param name="jsonActions">服务行为</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void AddRange(IEnumerable<JsonAction> jsonActions)
        {
            foreach (var action in jsonActions)
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
        public JsonAction TryGet(string name)
        {
            JsonAction jsonAction;
            if (this.dictionary.TryGetValue(name, out jsonAction))
            {
                return jsonAction;
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
