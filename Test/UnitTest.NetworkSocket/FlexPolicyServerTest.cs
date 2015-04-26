using NetworkSocket.Policies;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace UnitTest.NetworkSocket
{


    /// <summary>
    ///这是 FlexPolicyServerTest 的测试类，旨在
    ///包含所有 FlexPolicyServerTest 单元测试
    ///</summary>
    [TestClass()]
    public class FlexPolicyServerTest
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


        /// <summary>
        ///StartListen 的测试
        ///</summary>
        [TestMethod()]
        public void StartListenTest()
        {
            FlexPolicyServer target = new FlexPolicyServer(); // TODO: 初始化为适当的值
            target.StartListen();
            Assert.IsTrue(target.IsListening == true && target.Port == 843);
            target.Dispose();
        }

        /// <summary>
        /// 授权 的测试
        ///</summary>
        [TestMethod()]
        public void PolicyTest()
        {
            FlexPolicyServer target = new FlexPolicyServer(); // TODO: 初始化为适当的值
            target.StartListen();

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new System.Net.IPEndPoint(IPAddress.Loopback, 843));
            socket.Send(new byte[] { 1, 2 });

            byte[] buffer = new byte[1024 * 8];
            var count = socket.Receive(buffer, SocketFlags.None);
            Assert.IsTrue(count > 0);
            var policy = Encoding.UTF8.GetString(buffer, 0, count);
            Assert.IsTrue(policy.StartsWith("<cross"));
        }
    }
}
