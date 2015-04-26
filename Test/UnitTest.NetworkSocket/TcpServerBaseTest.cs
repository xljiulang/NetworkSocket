using NetworkSocket.Policies;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using NetworkSocket;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Linq;

namespace UnitTest.NetworkSocket
{


    /// <summary>
    ///这是 FlexPolicyServerTest 的测试类，旨在
    ///包含所有 FlexPolicyServerTest 单元测试
    ///</summary>
    [TestClass()]
    public class TcpServerBaseTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///获取或设置测试上下文，上下文提供
        ///有关当前测试运行及其功能的信息。
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region 附加测试特性
        // 
        //编写测试时，还可使用以下特性:
        //
        //使用 ClassInitialize 在运行类中的第一个测试前先运行代码
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //使用 ClassCleanup 在运行完类中的所有测试后再运行代码
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //使用 TestInitialize 在运行每个测试前先运行代码
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //使用 TestCleanup 在运行完每个测试后运行代码
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        class MyTcpServerBase : TcpServerBase<SessionBase>
        {
            public int ConnectTimes = 0;
            public int DisConnectTimes = 0;

            protected override SessionBase OnCreateSession()
            {
                return new SessionBase();
            }

            protected override void OnReceive(SessionBase session, ReceiveBuffer buffer)
            {
                buffer.Clear();
            }

            protected override void OnConnect(SessionBase session)
            {
                System.Threading.Interlocked.Increment(ref this.ConnectTimes);
                base.OnConnect(session);
            }

            protected override void OnDisconnect(SessionBase session)
            {
                System.Threading.Interlocked.Increment(ref this.DisConnectTimes);
                base.OnDisconnect(session);
            }
        }

        /// <summary>
        ///StartListen 的测试
        ///</summary>
        [TestMethod()]
        public void StartListenTest()
        {
            MyTcpServerBase target = new MyTcpServerBase(); // TODO: 初始化为适当的值
            target.StartListen(6600);
            Assert.IsTrue(target.IsListening);
            target.Dispose();
        }


        /// <summary>
        ///OnConnect 的测试
        ///</summary>
        [TestMethod()]
        public void OnConnectTest()
        {
            MyTcpServerBase target = new MyTcpServerBase(); // TODO: 初始化为适当的值
            target.StartListen(6611);

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new System.Net.IPEndPoint(IPAddress.Loopback, 6611));
            Thread.Sleep(50);
            Assert.IsTrue(target.ConnectTimes == 1);
            Assert.IsTrue(target.AllSessions.Count() == 1);
            Assert.IsTrue(target.ExtraState.FreeSessionCount == 0);


            var socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket2.Connect(new System.Net.IPEndPoint(IPAddress.Loopback, 6611));
            Thread.Sleep(50);
            Assert.IsTrue(target.ConnectTimes == 2);
            Assert.IsTrue(target.AllSessions.Count() == 2);
            Assert.IsTrue(target.ExtraState.FreeSessionCount == 0);


            socket.Dispose();
            Thread.Sleep(50);
            Assert.IsTrue(target.DisConnectTimes == 1);
            Assert.IsTrue(target.AllSessions.Count() == 1);
            Assert.IsTrue(target.ExtraState.FreeSessionCount == 1);

            socket2.Dispose();
            Thread.Sleep(50);
            Assert.IsTrue(target.DisConnectTimes == 2);
            Assert.IsTrue(target.AllSessions.Count() == 0);
            Assert.IsTrue(target.ExtraState.FreeSessionCount == 2);
           
            target.Dispose();
        }
    }
}
