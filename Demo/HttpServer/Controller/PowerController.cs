using HttpServer.Filters;
using NetworkSocket.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace HttpServer.Controller
{
    /// <summary>
    /// 电源控制器
    /// </summary>
    public class PowerController : HttpController
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }

        [Flags]
        private enum ExitCode : uint
        {
            LogOff = 0x00,
            ShutDown = 0x01,
            Reboot = 0x02,
            Force = 0x04,
            PowerOff = 0x08,
            ForceIfHung = 0x10,
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll")]
        private static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);

        [DllImport("advapi32.dll")]
        private static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [DllImport("advapi32.dll")]
        private static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("user32.dll")]
        private static extern bool ExitWindowsEx(ExitCode code, int ret);

        /// <summary>
        /// 调整自身令牌
        /// </summary>
        /// <returns></returns>
        private static bool AdjustSelfToken()
        {
            const int TOKEN_QUERY = 0x00000008;
            const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
            const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";

            var hTok = IntPtr.Zero;
            if (OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref hTok) == false)
            {
                return false;
            }

            var tokp = new TokPriv1Luid
            {
                Count = 1,
                Luid = 0,
                Attr = 0x00000002
            };
            if (LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref tokp.Luid) == false)
            {
                return false;
            }
            return AdjustTokenPrivileges(hTok, false, ref tokp, 0, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// 退出window
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private static bool ExitWindows(ExitCode code)
        {
            return AdjustSelfToken() && ExitWindowsEx(code, 0);
        }

        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var view = System.IO.File.ReadAllText("View\\Power\\Index.cshtml", Encoding.Default);
            return Content(view);
        }

        /// <summary>
        /// 重启
        /// </summary>
        /// <returns></returns>
        [LogFilter("重启")]
        public JsonResult Reboot()
        {
            var state = ExitWindows(ExitCode.Reboot | ExitCode.ForceIfHung);
            return Json(state);
        }

        /// <summary>
        /// 关机
        /// </summary>
        /// <returns></returns>
        [LogFilter("关机")]
        public JsonResult Shutdown()
        {
            var state = ExitWindows(ExitCode.ShutDown | ExitCode.ForceIfHung);
            return Json(state);
        }

        /// <summary>
        /// 注销
        /// </summary>
        /// <returns></returns>
        [LogFilter("注销")]
        public JsonResult Logoff()
        {
            var state = ExitWindows(ExitCode.LogOff | ExitCode.ForceIfHung);
            return Json(state);
        }
    }
}
