using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSocket;

namespace ClientTest
{
    /// <summary>
    /// SilverLight授权服务封包协议
    /// </summary>
    public class PolicyPacket : PacketBase
    {
        public Byte[] Buffer { get; private set; }

        public PolicyPacket(byte[] bytes)
        {
            this.Buffer = bytes;
        }

        public override byte[] ToByteArray()
        {
            return this.Buffer;
        }

        public static PolicyPacket GetPacket(ByteBuilder builder)
        {
            if (builder.Length == 0) return null;

            var bytes = builder.ToArray();
            builder.Clear();
            return new PolicyPacket(bytes);
        }

    }

    public class Client : TcpClientBase<PolicyPacket>
    {
        private System.Threading.Timer timer;

        private int totalCount = 0;

        public Client()
        {
            this.timer = new System.Threading.Timer((state) =>
            {
                Console.WriteLine(totalCount);
                totalCount = 0;
            }, null, 0, 1000);
        }

        protected override PolicyPacket OnReceive(ByteBuilder recvBuilder)
        {           
            return PolicyPacket.GetPacket(recvBuilder);
        }

        protected override void OnRecvComplete(PolicyPacket packet)
        {
            this.totalCount++;
            this.Send(packet);
        }
    }
}
