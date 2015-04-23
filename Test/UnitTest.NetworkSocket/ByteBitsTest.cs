using NetworkSocket;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTest.NetworkSocket
{


    /// <summary>
    ///这是 ByteBitsTest 的测试类，旨在
    ///包含所有 ByteBitsTest 单元测试
    ///</summary>
    [TestClass()]
    public class ByteBitsTest
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
        ///Equals 的测试
        ///</summary>
        [TestMethod()]
        public void EqualsTest()
        {
            ByteBits target = 5; // TODO: 初始化为适当的值
            object obj = 5; // TODO: 初始化为适当的值
            bool expected = false; // TODO: 初始化为适当的值
            bool actual;
            actual = target.Equals(obj);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("验证此测试方法的正确性。");
        }

        /// <summary>
        ///GetHashCode 的测试
        ///</summary>
        [TestMethod()]
        public void GetHashCodeTest()
        {
            ByteBits target = 6; // TODO: 初始化为适当的值
            int expected = 6; // TODO: 初始化为适当的值
            int actual;
            actual = target.GetHashCode();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("验证此测试方法的正确性。");
        }

        /// <summary>
        ///MoveLeft 的测试
        ///</summary>
        [TestMethod()]
        public void MoveLeftTest()
        {
            ByteBits target = byte.MaxValue; // TODO: 初始化为适当的值
            int count = 1; // TODO: 初始化为适当的值
            ByteBits expected = byte.MaxValue - 1; // TODO: 初始化为适当的值
            ByteBits actual;
            actual = target.MoveLeft(count);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("验证此测试方法的正确性。");
        }


        /// <summary>
        ///MoveRight 的测试
        ///</summary>
        [TestMethod()]
        public void MoveRightTest()
        {
            ByteBits target = byte.MaxValue; // TODO: 初始化为适当的值
            int count = 6; // TODO: 初始化为适当的值
            ByteBits expected = 3; // TODO: 初始化为适当的值
            ByteBits actual;
            actual = target.MoveRight(count);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("验证此测试方法的正确性。");
        }

        /// <summary>
        ///Take 的测试
        ///</summary>
        [TestMethod()]
        public void TakeTest()
        {
            ByteBits target = byte.MaxValue; // TODO: 初始化为适当的值
            int count = 4; // TODO: 初始化为适当的值
            ByteBits expected = 15; // TODO: 初始化为适当的值
            ByteBits actual;
            actual = target.Take(count);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("验证此测试方法的正确性。");
        }

        /// <summary>
        ///Take 的测试
        ///</summary>
        [TestMethod()]
        public void TakeTest1()
        {
            ByteBits target = 2; // TODO: 初始化为适当的值
            int index = 8 - 2; // TODO: 初始化为适当的值
            int count = 2; // TODO: 初始化为适当的值
            ByteBits expected = 2; // TODO: 初始化为适当的值
            ByteBits actual;
            actual = target.Take(index, count);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("验证此测试方法的正确性。");
        }



        /// <summary>
        ///op_Implicit 的测试
        ///</summary>
        [TestMethod()]
        public void op_ImplicitTest()
        {
            byte value = 0; // TODO: 初始化为适当的值
            ByteBits expected = new ByteBits(); // TODO: 初始化为适当的值
            ByteBits actual;
            actual = value;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("验证此测试方法的正确性。");
        }

        /// <summary>
        ///op_Implicit 的测试
        ///</summary>
        [TestMethod()]
        public void op_ImplicitTest1()
        {
            ByteBits bits = new ByteBits(); // TODO: 初始化为适当的值
            byte expected = 0; // TODO: 初始化为适当的值
            byte actual;
            actual = bits;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("验证此测试方法的正确性。");
        }

        /// <summary>
        ///Item 的测试
        ///</summary>
        [TestMethod()]
        public void ItemTest()
        {
            ByteBits target = new ByteBits(); // TODO: 初始化为适当的值
            int index = 0; // TODO: 初始化为适当的值
            bool expected = false; // TODO: 初始化为适当的值
            bool actual;
            target[index] = expected;
            actual = target[index];
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("验证此测试方法的正确性。");
        }
    }
}
