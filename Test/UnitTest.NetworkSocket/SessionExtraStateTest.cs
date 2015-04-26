using NetworkSocket;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTest.NetworkSocket
{
    
    
    /// <summary>
    ///这是 SessionExtraStateTest 的测试类，旨在
    ///包含所有 SessionExtraStateTest 单元测试
    ///</summary>
    [TestClass()]
    public class SessionExtraStateTest
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
        ///RecvTimes 的测试
        ///</summary>
        [TestMethod()]
        public void RecvTimesTest()
        {
            SessionExtraState target = new SessionExtraState(); // TODO: 初始化为适当的值
            long actual;
            target.SetRecved(3);
            target.SetRecved(3);
            actual = target.RecvTimes;
            Assert.IsTrue(actual == 2);
        }

        /// <summary>
        ///SendTimes 的测试
        ///</summary>
        [TestMethod()]
        public void SendTimesTest()
        {
            SessionExtraState target = new SessionExtraState(); // TODO: 初始化为适当的值
            long actual;
            target.SetSended(1);
            target.SetSended(1);
            actual = target.SendTimes;
            Assert.IsTrue(actual == 2);
        }

        /// <summary>
        ///TotalRecvByteCount 的测试
        ///</summary>
        [TestMethod()]
        public void TotalRecvByteCountTest()
        {
            SessionExtraState target = new SessionExtraState(); // TODO: 初始化为适当的值
            long actual;
            target.SetRecved(3);
            target.SetRecved(5);
            actual = target.TotalRecvByteCount;
            Assert.IsTrue(actual == 8);
        }

        /// <summary>
        ///TotalSendByteCount 的测试
        ///</summary>
        [TestMethod()]
        public void TotalSendByteCountTest()
        {
            SessionExtraState target = new SessionExtraState(); // TODO: 初始化为适当的值
            long actual;
            target.SetSended(3);
            target.SetSended(5);
            actual = target.TotalSendByteCount;
            Assert.IsTrue(actual == 8);
        }

        /// <summary>
        ///RecvTimes 的测试
        ///</summary>
        [TestMethod()]
        public void RecvTimesTest1()
        {
            SessionExtraState target = new SessionExtraState(); // TODO: 初始化为适当的值
            long actual;
            target.SetRecved(3);
            target.SetRecved(5);
            actual = target.RecvTimes;
            Assert.IsTrue(actual == 2);
        }

        /// <summary>
        ///SendTimes 的测试
        ///</summary>
        [TestMethod()]
        public void SendTimesTest1()
        {
            SessionExtraState target = new SessionExtraState(); // TODO: 初始化为适当的值
            long actual;
            target.SetSended(3);
            target.SetSended(5);
            actual = target.SendTimes;
            Assert.IsTrue(actual == 2);
        }      
    }
}
