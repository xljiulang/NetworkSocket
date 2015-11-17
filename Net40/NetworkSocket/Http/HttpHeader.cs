using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NetworkSocket.Http
{
    /// <summary>
    /// http头
    /// </summary>
    public class HttpHeader : HttpNameValueCollection
    {
        /// <summary>
        /// http头
        /// </summary>       
        public HttpHeader(CaptureCollection keys, CaptureCollection values)
        {
            for (var i = 0; i < keys.Count; i++)
            {
                var name = keys[i].ToString();
                var value = values[i].ToString();
                this.Add(name, value);
            }
        }

        /// <summary>
        /// 获取内容长度
        /// </summary>
        public int ContentLength
        {
            get
            {
                return this.TryGet<int>("Content-Length");
            }
        }

        /// <summary>
        /// 获取用户代理字符串
        /// </summary>
        public string UserAgent
        {
            get
            {
                return this.TryGet<string>("User-Agent");
            }
        }

        /// <summary>
        /// 获取指定键的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        public T TryGet<T>(string key)
        {
            return this.TryGet<T>(key, default(T));
        }

        /// <summary>
        /// 获取指定键的值
        /// 失败则返回defaultValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public T TryGet<T>(string key, T defaultValue)
        {
            var value = this[key];
            if (string.IsNullOrEmpty(value) == true)
            {
                return defaultValue;
            }

            try
            {
                return (T)((IConvertible)value).ToType(typeof(T), null);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}
