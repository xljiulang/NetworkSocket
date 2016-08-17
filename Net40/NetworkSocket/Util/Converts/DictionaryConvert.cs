using NetworkSocket.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSocket.Util.Converts
{
    /// <summary>
    /// 表示字典转换单元
    /// </summary>
    public class DictionaryConvert : IConvert
    {
        /// <summary>
        /// 转换器实例
        /// </summary>
        public Converter Converter { get; set; }

        /// <summary>
        /// 下一个转换单元
        /// </summary>
        public IConvert NextConvert { get; set; }

        /// <summary>
        /// 将value转换为目标类型
        /// </summary>
        /// <param name="value">要转换的值</param>
        /// <param name="targetType">转换的目标类型</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType)
        {
            var dic = value as IDictionary<string, object>;
            if (dic == null)
            {
                return this.NextConvert.Convert(value, targetType);
            }

            var instance = Activator.CreateInstance(targetType);
            var setters = Property.GetProperties(targetType);

            foreach (var set in setters)
            {
                if (set.Info.CanWrite == false)
                {
                    continue;
                }

                var key = dic.Keys.FirstOrDefault(k => string.Equals(k, set.Name, StringComparison.OrdinalIgnoreCase));
                if (key != null)
                {
                    var targetValue = this.Converter.Convert(dic[key], set.Info.PropertyType);
                    set.SetValue(instance, targetValue);
                }
            }

            return instance;
        }
    }
}
