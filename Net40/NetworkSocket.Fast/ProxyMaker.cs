using NetworkSocket;
using NetworkSocket.Fast;
using NetworkSocket.Fast.Attributes;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace NetworkSocket.Fast
{
    /// <summary>
    /// 客户端代理生成
    /// </summary>
    internal sealed class ProxyMaker
    {
        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// 方法代码
        /// </summary>
        public List<string> MethodCodeList { get; set; }

        /// <summary>
        /// 客户端代理生成
        /// </summary>
        /// <param name="type">服务类型</param>
        public ProxyMaker(Type type)
        {
            this.ClassName = type.Name;
            this.MethodCodeList = new List<string>();
            this.MakeCode(type);
        }

        /// <summary>
        /// 获取所有方法的代理代码
        /// </summary>
        /// <param name="type">服务类型</param>
        /// <returns></returns>
        private void MakeCode(Type type)
        {
            var methods = type.GetMethods().Where(item => Attribute.IsDefined(item, typeof(ServiceAttribute)));
            foreach (var m in methods)
            {
                var service = Attribute.GetCustomAttribute(m, typeof(ServiceAttribute)) as ServiceAttribute;
                if (service.Implement == Implements.Self)
                {
                    if (Enum.IsDefined(typeof(SpecialCommands), service.Command) == false)
                    {
                        var code = this.GetSelfMethodCode(m, service);
                        this.MethodCodeList.Add(code);
                    }
                }
                else
                {
                    var code = this.GetRemoteMethodCode(m, service);
                    this.MethodCodeList.Add(code);
                }
            }
        }


        /// <summary>
        /// 获取客端方法代码
        /// </summary>
        /// <param name="method">方法</param>   
        /// <param name="service">特性</param>
        /// <returns></returns>
        private string GetRemoteMethodCode(MethodInfo method, ServiceAttribute service)
        {
            var returnType = typeof(void);
            if (method.ReturnType.IsGenericType)
            {
                returnType = method.ReturnType.GetGenericArguments().First();
            }

            var paramters = method.GetParameters().Skip(1)
                .Select(item => string.Format("{0} {1}", this.GetTypeName(item.ParameterType), item.Name));

            var attribute = this.FormatString(4, "[Service(Implements.Self, {0})]", service.Command);
            var statement = this.FormatString(4, "public abstract {0} {1}({2});", this.GetTypeName(returnType), method.Name, string.Join(", ", paramters));
            return attribute + "\r\n" + statement;
        }


        /// <summary>
        /// 生成服务方法代码
        /// </summary>
        /// <param name="method">方法</param>
        /// <param name="service">特性</param>      
        /// <returns></returns>
        private string GetSelfMethodCode(MethodInfo method, ServiceAttribute service)
        {
            var returnTypeName = this.GetTypeName(method.ReturnType);
            var parameters = method.GetParameters().Skip(1);
            var parameterStateString = string.Join(", ", parameters.Select(item => string.Format("{0} {1}", this.GetTypeName(item.ParameterType), item.Name)));
            var parameterItemsString = string.Join(", ", parameters.Select(item => item.Name));
            if (string.IsNullOrEmpty(parameterItemsString) == false)
            {
                parameterItemsString = ", " + parameterItemsString;
            }
            var attribute = this.FormatString(4, "[Service(Implements.Remote, {0})]", service.Command);
            var statement = this.FormatString(4, "public {0} {1}({2})", returnTypeName, method.Name, parameterStateString);
            if (method.ReturnType != typeof(void))
            {
                statement = this.FormatString(4, "public Task<{0}> {1}({2})", returnTypeName, method.Name, parameterStateString);
            }
            var inline = this.FormatString(8, "this.InvokeRemote({0}{1});", service.Command, parameterItemsString);
            if (method.ReturnType != typeof(void))
            {
                inline = this.FormatString(8, "return this.InvokeRemote<{0}>({1}{2});", returnTypeName, service.Command, parameterItemsString);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(attribute);
            sb.AppendLine(statement);
            sb.AppendLine(string.Empty.PadRight(4, ' ') + "{");
            sb.AppendLine(inline);
            sb.AppendLine(string.Empty.PadRight(4, ' ') + "}");
            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// 格式化字符串
        /// </summary>
        /// <param name="leftSpace">左空格</param>
        /// <param name="farmat">格式</param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        private string FormatString(int leftSpace, string farmat, params object[] args)
        {
            return string.Empty.PadRight(leftSpace, ' ') + string.Format(farmat, args);
        }

        /// <summary>
        /// 获取类型的字符串表达示
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        private string GetTypeName(Type type)
        {
            if (type.Equals(typeof(void)))
            {
                return "void";
            }
            else if (type.IsGenericType)
            {
                return string.Format(Regex.Replace(type.Name, @"`\d+", "<{0}>"), this.GetGenericTypeArgs(type));
            }
            else
            {
                return type.Name;
            }
        }

        /// <summary>
        /// 获取泛型参数
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        private string GetGenericTypeArgs(Type type)
        {
            Type[] typeArguments = type.GetGenericArguments();
            string args = string.Empty;
            foreach (var t in typeArguments)
            {
                args = args + this.GetTypeName(t) + ",";
            }
            return args.TrimEnd(',');
        }

        /// <summary>
        /// 生成代理代码
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("public abstract class @ClassNameProxyBase : FastTcpClientBase");
            sb.AppendLine("{");
            sb.AppendLine(string.Join("\r\n\r\n", this.MethodCodeList));
            sb.AppendLine("}");
            return sb.ToString().Replace("@ClassName", this.ClassName);
        }
    }
}
