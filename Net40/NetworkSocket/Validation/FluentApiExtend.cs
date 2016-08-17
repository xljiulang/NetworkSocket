using NetworkSocket.Validation.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace NetworkSocket.Validation
{
    /// <summary>
    /// 提供FluentApi扩展
    /// </summary>
    public static class FluentApiExtend
    {
        /// <summary>
        /// 获取表达式的属性
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <typeparam name="TKey">属性类型</typeparam>
        /// <param name="keySelector">属性选择</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <returns></returns>
        public static RuleProperty GetProperty<T, TKey>(Expression<Func<T, TKey>> keySelector)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }

            var body = keySelector.Body as MemberExpression;
            if (body == null)
            {
                throw new ArgumentException("表达式必须为MemberExpression ..", "keySelector");
            }

            if (body.Member.DeclaringType.IsAssignableFrom(typeof(T)) == false || body.Expression.NodeType != ExpressionType.Parameter)
            {
                throw new ArgumentException("无法解析的表达式 ..", "keySelector");
            }

            var propertyInfo = body.Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentException("表达式选择的字段不是属性 ..", "keySelector");
            }

            var property = RuleProperty
                .GetGetProperties(typeof(T))
                .FirstOrDefault(item => item.Info == propertyInfo);

            if (property == null)
            {
                throw new NotSupportedException(string.Format("属性{0}.{1}不支持验证 ..", typeof(T).Name, propertyInfo.Name));
            }
            return property;
        }

        /// <summary>
        /// 要求必须输入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="fluent"></param>
        /// <param name="keySelector">属性选择</param>
        /// <returns></returns>
        public static FluentApi<T> Required<T, TKey>(this FluentApi<T> fluent, Expression<Func<T, TKey>> keySelector)
        {
            return fluent.SetRule(keySelector, new RequiredAttribute());
        }

        /// <summary>
        /// 要求必须输入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="fluent"></param>
        /// <param name="keySelector">属性选择</param>
        /// <param name="errorMessage">错误提示消息</param>
        /// <returns></returns>
        public static FluentApi<T> Required<T, TKey>(this FluentApi<T> fluent, Expression<Func<T, TKey>> keySelector, string errorMessage)
        {
            return fluent.SetRule(keySelector, new RequiredAttribute { ErrorMessage = errorMessage });
        }


        /// <summary>
        /// 验证是邮箱格式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fluent"></param>
        /// <param name="keySelector">属性选择</param>
        /// <returns></returns>
        public static FluentApi<T> Email<T>(this FluentApi<T> fluent, Expression<Func<T, string>> keySelector)
        {
            return fluent.SetRule(keySelector, new EmailAttribute());
        }

        /// <summary>
        /// 验证是邮箱格式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fluent"></param>
        /// <param name="keySelector">属性选择</param>
        /// <param name="errorMessage">错误提示消息</param>
        /// <returns></returns>
        public static FluentApi<T> Email<T>(this FluentApi<T> fluent, Expression<Func<T, string>> keySelector, string errorMessage)
        {
            return fluent.SetRule(keySelector, new EmailAttribute { ErrorMessage = errorMessage });
        }

        /// <summary>
        /// 验证是否和目标属性的值一致
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TTargetKey"></typeparam>
        /// <param name="fluent"></param>
        /// <param name="keySelector">属性选择</param>
        /// <param name="targetKeySelector">目标属性选择</param>
        /// <returns></returns>
        public static FluentApi<T> EqualTo<T, TKey, TTargetKey>(this FluentApi<T> fluent, Expression<Func<T, TKey>> keySelector, Expression<Func<T, TTargetKey>> targetKeySelector)
        {
            var propertyName = FluentApiExtend.GetProperty(targetKeySelector).Name;
            return fluent.SetRule(keySelector, new EqualToAttribute(propertyName));
        }

        /// <summary>
        /// 验证是否和目标属性的值一致
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TTargetKey"></typeparam>
        /// <param name="fluent"></param>
        /// <param name="keySelector">属性选择</param>
        /// <param name="targetKeySelector">目标属性选择</param>
        /// <param name="errorMessage">错误提示信息</param>
        /// <returns></returns>
        public static FluentApi<T> EqualTo<T, TKey, TTargetKey>(this FluentApi<T> fluent, Expression<Func<T, TKey>> keySelector, Expression<Func<T, TTargetKey>> targetKeySelector, string errorMessage)
        {
            var propertyName = FluentApiExtend.GetProperty(targetKeySelector).Name;
            return fluent.SetRule(keySelector, new EqualToAttribute(propertyName) { ErrorMessage = errorMessage });
        }



        /// <summary>
        /// 验证输入的长度范围
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fluent"></param>
        /// <param name="keySelector">属性选择</param>
        /// <param name="maxLength">最大长度</param>
        /// <returns></returns>
        public static FluentApi<T> Length<T>(this FluentApi<T> fluent, Expression<Func<T, string>> keySelector, int maxLength)
        {
            return fluent.SetRule(keySelector, new LengthAttribute(maxLength));
        }

        /// <summary>
        /// 验证输入的长度范围
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fluent"></param>
        /// <param name="keySelector">属性选择</param>
        /// <param name="maxLength">最大长度</param>
        /// <param name="errorMessage">错误提示信息</param>
        /// <returns></returns>
        public static FluentApi<T> Length<T>(this FluentApi<T> fluent, Expression<Func<T, string>> keySelector, int maxLength, string errorMessage)
        {
            return fluent.SetRule(keySelector, new LengthAttribute(maxLength) { ErrorMessage = errorMessage });
        }

        /// <summary>
        /// 验证输入的长度范围
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fluent"></param>
        /// <param name="keySelector">属性选择</param>
        /// <param name="maxLength">最大长度</param>
        /// <param name="minValue">最小长度</param>
        /// <returns></returns>
        public static FluentApi<T> Length<T>(this FluentApi<T> fluent, Expression<Func<T, string>> keySelector, int maxLength, int minValue)
        {
            return fluent.SetRule(keySelector, new LengthAttribute(maxLength) { MinimumLength = minValue });
        }

        /// <summary>
        /// 验证输入的长度范围
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fluent"></param>
        /// <param name="keySelector">属性选择</param>
        /// <param name="maxLength">最大长度</param>
        /// <param name="minValue">最小长度</param>
        /// <param name="errorMessage">错误提示消息</param>
        /// <returns></returns>
        public static FluentApi<T> Length<T>(this FluentApi<T> fluent, Expression<Func<T, string>> keySelector, int maxLength, int minValue, string errorMessage)
        {
            return fluent.SetRule(keySelector, new LengthAttribute(maxLength) { MinimumLength = minValue, ErrorMessage = errorMessage });
        }



        /// <summary>
        /// 验证是否和正则表达式匹配
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fluent"></param>
        /// <param name="keySelector">属性选择</param>
        /// <param name="pattern">正则表达式</param>
        /// <returns></returns>
        public static FluentApi<T> Match<T>(this FluentApi<T> fluent, Expression<Func<T, string>> keySelector, string pattern)
        {
            return fluent.SetRule(keySelector, new MatchAttribute(pattern));
        }

        /// <summary>
        /// 验证是否和正则表达式匹配
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fluent"></param>
        /// <param name="keySelector">属性选择</param>
        /// <param name="pattern">正则表达式</param>
        /// <param name="errorMessage">错误提示信息</param>
        /// <returns></returns>
        public static FluentApi<T> Match<T>(this FluentApi<T> fluent, Expression<Func<T, string>> keySelector, string pattern, string errorMessage)
        {
            return fluent.SetRule(keySelector, new MatchAttribute(pattern) { ErrorMessage = errorMessage });
        }



        /// <summary>
        /// 验证不要和目标属性的值一致
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TTargetKey"></typeparam>
        /// <param name="fluent"></param>
        /// <param name="keySelector">属性选择</param>
        /// <param name="targetKeySelector">目标属性选择</param>
        /// <returns></returns>
        public static FluentApi<T> NotEqualTo<T, TKey, TTargetKey>(this FluentApi<T> fluent, Expression<Func<T, TKey>> keySelector, Expression<Func<T, TTargetKey>> targetKeySelector)
        {
            var propertyName = FluentApiExtend.GetProperty(targetKeySelector).Name;
            return fluent.SetRule(keySelector, new NotEqualToAttribute(propertyName));
        }

        /// <summary>
        /// 验证不要和目标属性的值一致
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TTargetKey"></typeparam>
        /// <param name="fluent"></param>
        /// <param name="keySelector">属性选择</param>
        /// <param name="targetKeySelector">目标属性选择</param>
        /// <param name="errorMessage">错误提示信息</param>
        /// <returns></returns>
        public static FluentApi<T> NotEqualTo<T, TKey, TTargetKey>(this FluentApi<T> fluent, Expression<Func<T, TKey>> keySelector, Expression<Func<T, TTargetKey>> targetKeySelector, string errorMessage)
        {
            var propertyName = FluentApiExtend.GetProperty(targetKeySelector).Name;
            return fluent.SetRule(keySelector, new NotEqualToAttribute(propertyName) { ErrorMessage = errorMessage });
        }



        /// <summary>
        /// 表示验证不要和正则表达式匹配
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fluent"></param>
        /// <param name="keySelector">属性选择</param>
        /// <param name="pattern">正则表达式</param>
        /// <returns></returns>
        public static FluentApi<T> NotMatch<T>(this FluentApi<T> fluent, Expression<Func<T, string>> keySelector, string pattern)
        {
            return fluent.SetRule(keySelector, new NotMatchAttribute(pattern));
        }

        /// <summary>
        /// 表示验证不要和正则表达式匹配
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fluent"></param>
        /// <param name="keySelector">属性选择</param>
        /// <param name="pattern">正则表达式</param>
        /// <param name="errorMessage">错误提示信息</param>
        /// <returns></returns>
        public static FluentApi<T> NotMatch<T>(this FluentApi<T> fluent, Expression<Func<T, string>> keySelector, string pattern, string errorMessage)
        {
            return fluent.SetRule(keySelector, new NotMatchAttribute(pattern) { ErrorMessage = errorMessage });
        }



        /// <summary>
        /// 表示精度验证
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="fluent"></param>
        /// <param name="keySelector">属性选择</param>
        /// <param name="min">最小精度</param>
        /// <param name="max">最大精度</param>
        /// <returns></returns>
        public static FluentApi<T> Precision<T, TKey>(this FluentApi<T> fluent, Expression<Func<T, TKey>> keySelector, int min, int max)
        {
            return fluent.SetRule(keySelector, new PrecisionAttribute(min, max));
        }


        /// <summary>
        /// 表示精度验证
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="fluent"></param>
        /// <param name="keySelector">属性选择</param>
        /// <param name="min">最小精度</param>
        /// <param name="max">最大精度</param>
        /// <param name="errorMessage">错误提示信息</param>
        /// <returns></returns>
        public static FluentApi<T> Precision<T, TKey>(this FluentApi<T> fluent, Expression<Func<T, TKey>> keySelector, int min, int max, string errorMessage)
        {
            return fluent.SetRule(keySelector, new PrecisionAttribute(min, max) { ErrorMessage = errorMessage });
        }




        /// <summary>
        /// 表示验值要在一定的区间中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="fluent"></param>
        /// <param name="keySelector">属性选择</param>
        /// <param name="minValue">最小值</param>
        /// <param name="maxValue">最大值</param>
        /// <returns></returns>
        public static FluentApi<T> Range<T, TKey>(this FluentApi<T> fluent, Expression<Func<T, TKey>> keySelector, int minValue, int maxValue)
        {
            return fluent.SetRule(keySelector, new RangeAttribute(minValue, maxValue));
        }

        /// <summary>
        /// 表示验值要在一定的区间中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="fluent"></param>
        /// <param name="keySelector">属性选择</param>
        /// <param name="minValue">最小值</param>
        /// <param name="maxValue">最大值</param>
        /// <returns></returns>
        public static FluentApi<T> Range<T, TKey>(this FluentApi<T> fluent, Expression<Func<T, TKey>> keySelector, double minValue, double maxValue)
        {
            return fluent.SetRule(keySelector, new RangeAttribute(minValue, maxValue));
        }

        /// <summary>
        /// 表示验值要在一定的区间中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="fluent"></param>
        /// <param name="keySelector">属性选择</param>
        /// <param name="minValue">最小值</param>
        /// <param name="maxValue">最大值</param>
        /// <param name="errorMessage">错误提示信息</param>
        /// <returns></returns>
        public static FluentApi<T> Range<T, TKey>(this FluentApi<T> fluent, Expression<Func<T, TKey>> keySelector, int minValue, int maxValue, string errorMessage)
        {
            return fluent.SetRule(keySelector, new RangeAttribute(minValue, maxValue) { ErrorMessage = errorMessage });
        }

        /// <summary>
        /// 表示验值要在一定的区间中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="fluent"></param>
        /// <param name="keySelector">属性选择</param>
        /// <param name="minValue">最小值</param>
        /// <param name="maxValue">最大值</param>
        /// <param name="errorMessage">错误提示信息</param>
        /// <returns></returns>
        public static FluentApi<T> Range<T, TKey>(this FluentApi<T> fluent, Expression<Func<T, TKey>> keySelector, double minValue, double maxValue, string errorMessage)
        {
            return fluent.SetRule(keySelector, new RangeAttribute(minValue, maxValue) { ErrorMessage = errorMessage });
        }


        /// <summary>
        /// 表示验证是网络地址
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fluent"></param>
        /// <param name="keySelector">属性选择</param>
        /// <returns></returns>
        public static FluentApi<T> Url<T>(this FluentApi<T> fluent, Expression<Func<T, string>> keySelector)
        {
            return fluent.SetRule(keySelector, new UrlAttribute());
        }

        /// <summary>
        /// 表示验证是网络地址
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fluent"></param>
        /// <param name="keySelector">属性选择</param>
        /// <param name="errorMessage">错误提示信息</param>
        /// <returns></returns>
        public static FluentApi<T> Url<T>(this FluentApi<T> fluent, Expression<Func<T, string>> keySelector, string errorMessage)
        {
            return fluent.SetRule(keySelector, new UrlAttribute() { ErrorMessage = errorMessage });
        }
    }
}
