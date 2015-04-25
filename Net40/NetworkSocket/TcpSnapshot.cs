using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Net;
using System.Diagnostics;

namespace NetworkSocket
{
    /// <summary>
    /// 提供获取Tcp端口快照信息
    /// </summary>
    public static class TcpSnapshot
    {
        /// <summary>
        /// 表示端口的占用进程的id
        /// </summary>
        [DebuggerDisplay("Port = {Port}, OwerPid = {OwerPid}")]
        public class PortOwnerPid
        {
            /// <summary>
            /// 获取Tcp端口
            /// </summary>
            public int Port { get; internal set; }

            /// <summary>
            /// 获取占用端口的进程id
            /// </summary>
            public int OwerPid { get; internal set; }

            /// <summary>
            /// 获取哈希码
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return this.Port.GetHashCode() ^ this.OwerPid.GetHashCode();
            }

            /// <summary>
            /// 比较是否相等
            /// </summary>
            /// <param name="obj">目标对象</param>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                return obj != null && this.GetHashCode() == obj.GetHashCode();
            }
        }

        /// <summary>
        /// 端口进程信息
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct MIB_TCPROW_OWNER_PID
        {
            public uint State;
            public uint LocalAddr;
            public uint LocalPort;
            public uint RemoteAddr;
            public byte RemotePort;
            public uint OwningPid;
        }

        [DllImport("iphlpapi.dll", SetLastError = true)]
        private unsafe static extern uint GetExtendedTcpTable(
            void* pTcpTable,
            int* dwOutBufLen,
            bool sort,
            int ipVersion,
            int tblEnum,
            int reserved);


        /// <summary>
        /// 获取一次Tcp端口快照信息
        /// </summary>
        /// <returns></returns>
        public unsafe static PortOwnerPid[] Snapshot()
        {
            var hashSet = new HashSet<PortOwnerPid>();

            const int AF_INET = 2;
            const int TCP_TABLE_OWNER_PID_ALL = 5;

            var size = 0;
            TcpSnapshot.GetExtendedTcpTable(null, &size, true, AF_INET, TCP_TABLE_OWNER_PID_ALL, 0);
            byte* pTable = stackalloc byte[size];

            if (TcpSnapshot.GetExtendedTcpTable(pTable, &size, true, AF_INET, TCP_TABLE_OWNER_PID_ALL, 0) == 0)
            {
                var rowLength = *(int*)(pTable);
                var pRow = pTable + Marshal.SizeOf(rowLength);

                for (int i = 0; i < rowLength; i++)
                {
                    var row = *(MIB_TCPROW_OWNER_PID*)pRow;
                    var portOwner = new PortOwnerPid
                    {
                        Port = IPAddress.NetworkToHostOrder((short)row.LocalPort),
                        OwerPid = (int)row.OwningPid
                    };
                    hashSet.Add(portOwner);
                    pRow = pRow + Marshal.SizeOf(typeof(MIB_TCPROW_OWNER_PID));
                }
            }
            return hashSet.OrderBy(item => item.Port).ToArray();
        }
    }
}
