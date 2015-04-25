using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace NetworkSocket
{
    /// <summary>
    /// 表示用户附加数据
    /// </summary>    
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    internal sealed class TagBag : DynamicObject
    {
        /// <summary>
        /// 原始数据字典
        /// </summary>
        private TagData tagData;

        /// <summary>
        /// 用户附加数据
        /// </summary>
        /// <param name="tagData">用户附加数据</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TagBag(TagData tagData)
        {
            if (tagData == null)
            {
                throw new ArgumentNullException("tagData");
            }
            this.tagData = tagData;
        }
        /// <summary>
        /// 获取成员的值
        /// </summary>
        /// <param name="binder">成员</param>
        /// <param name="result">结果</param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this.tagData.TryGet(binder.Name);
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
            this.tagData.Set(binder.Name, value);
            return true;
        }

        /// <summary>
        /// 调试视图
        /// </summary>
        private class DebugView
        {
            /// <summary>
            /// 查看的对象
            /// </summary>
            private TagBag view;

            /// <summary>
            /// 调试视图
            /// </summary>
            /// <param name="view">查看的对象</param>
            public DebugView(TagBag view)
            {
                this.view = view;
            }

            /// <summary>
            /// 查看的内容
            /// </summary>
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public KeyValuePair<string, object>[] Values
            {
                get
                {
                    return this.view.tagData.KeyValues.ToArray();
                }
            }
        }
    }
}
