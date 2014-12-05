using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 表示动态数据字典
    /// </summary>    
    internal sealed class TagBag : DynamicObject
    {
        /// <summary>
        /// 原始数据字典
        /// </summary>
        private Dictionary<string, object> dic = new Dictionary<string, object>();

        /// <summary>
        /// 获取成员名称
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return this.dic.Keys;
        }

        /// <summary>
        /// 获取成员的值
        /// </summary>
        /// <param name="binder">成员</param>
        /// <param name="result">结果</param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            dic.TryGetValue(binder.Name, out result);
            return true;
        }

        /// <summary>
        /// 设置成员的值
        /// </summary>
        /// <param name="binder">成员</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            this.dic[binder.Name] = value;
            return true;
        }

        /// <summary>
        /// 清空内容
        /// </summary>
        public void Clear()
        {
            this.dic.Clear();
        }
    }
}
