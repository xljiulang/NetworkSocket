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
    public static class TcpTable
    {
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
        public unsafe static List<TcpPort> Snapshot()
        {
            var portList = new List<TcpPort>();

            const int AF_INET = 2;
            const int TCP_TABLE_OWNER_PID_ALL = 5;

            var size = 0;
            TcpTable.GetExtendedTcpTable(null, &size, true, AF_INET, TCP_TABLE_OWNER_PID_ALL, 0);
            byte* pTable = stackalloc byte[size];

            if (TcpTable.GetExtendedTcpTable(pTable, &size, true, AF_INET, TCP_TABLE_OWNER_PID_ALL, 0) == 0)
            {
                var rowLength = *(int*)(pTable);
                var pRow = pTable + Marshal.SizeOf(rowLength);

                for (int i = 0; i < rowLength; i++)
                {
                    var row = *(MIB_TCPROW_OWNER_PID*)pRow;
                    var tcpPort = TcpPort.FromTcpRow(row);

                    if (portList.Contains(tcpPort) == false)
                    {
                        portList.Add(tcpPort);
                    }

                    pRow = pRow + Marshal.SizeOf(typeof(MIB_TCPROW_OWNER_PID));
                }
            }

            return portList;
        }



        /// <summary>
        /// Tcp端口信息
        /// </summary>
        public sealed class TcpPort
        {
            /// <summary>
            /// 获取相关端口
            /// </summary>
            public int Port { get; private set; }

            /// <summary>
            /// 获取所占用的进程ID
            /// </summary>
            public int OwnerPId { get; private set; }

            /// <summary>
            /// 端口进程占用者信息
            /// </summary>
            /// <param name="port">端口</param>
            /// <param name="pid">进程ID</param>          
            private TcpPort(int port, int pid)
            {
                this.Port = port;
                this.OwnerPId = pid;
            }

            /// <summary>
            /// 从MIB_TCPROW_OWNER_PID对象获得数据
            /// </summary>
            /// <param name="row">MIB_TCPROW_OWNER_PID</param>
            /// <returns></returns>
            internal static TcpPort FromTcpRow(MIB_TCPROW_OWNER_PID row)
            {
                int port = IPAddress.NetworkToHostOrder((short)row.LocalPort);
                var pid = (int)row.OwningPid;
                return new TcpPort(port, pid);
            }

            /// <summary>
            /// 杀死占用此端口的进程            
            /// </summary>
            public bool Kill()
            {
                try
                {
                    var owner = Process.GetProcessById(this.OwnerPId);
                    if (owner != null)
                    {
                        owner.Kill();
                    }
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            /// <summary>
            /// 获取哈希值
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return this.Port.GetHashCode() ^ this.OwnerPId.GetHashCode();
            }
        }
    }
}
