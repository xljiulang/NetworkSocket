using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSocket;
using NetworkSocket.Core;

namespace TelnetServer
{
    /// <summary>
    /// 表示Telnet服务
    /// </summary>
    class TelnetListener : TcpServerBase<TelnetSession>
    {
        /// <summary>
        /// api列表
        /// </summary>
        private readonly Dictionary<string, ApiAction> apiList;

        /// <summary>
        /// 当前会话
        /// </summary>
        [ThreadStatic]
        private static TelnetSession CurrentSession;

        /// <summary>
        /// Telnet服务
        /// </summary>
        public TelnetListener()
        {
            this.apiList = new Dictionary<string, ApiAction>(StringComparer.OrdinalIgnoreCase);
            this.FillApis();
        }

        /// <summary>
        /// 反射查找API
        /// </summary>
        private void FillApis()
        {
            var apis = this
                .GetType()
                .GetMethods()
                .Where(m => m.IsDefined(typeof(ApiAttribute), false))
                .Select(m => new ApiAction(m));

            foreach (var api in apis)
            {
                this.apiList.Add(api.ApiName, api);
            }
        }

        /// <summary>
        /// 新建会话实例
        /// </summary>
        /// <returns></returns>
        protected sealed override TelnetSession OnCreateSession()
        {
            return new TelnetSession();
        }

        /// <summary>
        /// 有会话连接
        /// </summary>
        /// <param name="session">会话</param>
        protected sealed override void OnConnect(TelnetSession session)
        {
            session.Send("Welcome 2 telnet!We supported these commands:");
            foreach (var api in this.apiList)
            {
                session.Send(api.Key);
            }
        }

        /// <summary>
        /// 收到会话发来数据
        /// </summary>
        /// <param name="session">会话</param>
        /// <param name="buffer">收到的历史数据</param>
        protected sealed override void OnReceive(TelnetSession session, ReceiveStream buffer)
        {
            while (true)
            {
                var request = TelnetRequest.Parse(buffer);
                if (request == null)
                {
                    break;
                }
                this.ExecTelnetRequest(session, request);
            }
        }

        /// <summary>
        /// 执行Telnet请求        
        /// </summary>
        /// <param name="session">传话</param>
        /// <param name="request">请求</param>
        private void ExecTelnetRequest(TelnetSession session, TelnetRequest request)
        {
            var api = default(ApiAction);
            if (this.apiList.TryGetValue(request.Command, out api) == false)
            {
                session.Send("not supported command ..");
            }
            else
            {
                // 在工作线程中执行业务代码，当前线程为Socket的IO线程
                LimitedTask.Factory.StartNew(() => this.ExecApi(session, api, request.Argument));
            }
        }

        /// <summary>
        /// 反射执行Api
        /// 作用：扩展和维护都很方便
        /// 到了这一步，剩下的都是写业务代码了
        /// </summary>
        /// <param name="session"></param>
        /// <param name="api"></param>        
        /// <param name="arg"></param>
        private void ExecApi(TelnetSession session, ApiAction api, TelnetArgument arg)
        {
            CurrentSession = session;
            var result = api.Execute(this, arg);
            if (api.IsVoidReturn == false)
            {
                session.Send(Environment.NewLine + result);
            }
        }


        [Api]
        public Version Version(TelnetArgument arg)
        {
            return typeof(SessionBase).Assembly.GetName().Version;
        }


        [Api]
        public void Close(TelnetArgument arg)
        {
            this.Quit(arg);
        }

        [Api]
        public void Quit(TelnetArgument arg)
        {
            CurrentSession.Close();
        }

        [Api] // add 1 2 3
        public int Add(TelnetArgument arg)
        {
            return arg.GetArguments<int>().Sum();
        }

        [Api]
        public int Mult(TelnetArgument arg)
        {
            var result = 1;
            arg.GetArguments<int>().ToList().ForEach(item => result = result * item);
            return result;
        }
    }
}
