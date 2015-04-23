using NetworkSocket;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;

namespace UnitTest.NetworkSocket
{


    /// <summary>
    ///这是 ByteConverterTest 的测试类，旨在
    ///包含所有 ByteConverterTest 单元测试
    ///</summary>
    [TestClass()]
    public class ByteConverterTest
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

        private bool ByetesEqual(byte[] b1, byte[] b2)
        {
            if (b1 == null || b2 == null || b1.Length != b2.Length)
            {
                return false;
            }

            for (var i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i])
                {
                    return false;
                }
            }

            return true;
        }

        void AssertEqual(byte[] b1, byte[] b2)
        {
            Assert.IsTrue(ByetesEqual(b1, b2));
        }

        /// <summary>
        ///ToBytes 的测试
        ///</summary>
        [TestMethod()]
        public void ToBytesTest()
        {
            long value = 5; // TODO: 初始化为适当的值
            Endians endian = Endians.Little; // TODO: 初始化为适当的值
            byte[] expected = BitConverter.GetBytes(value); // TODO: 初始化为适当的值
            byte[] actual;
            actual = ByteConverter.ToBytes(value, endian);
            AssertEqual(expected, actual);

            endian = Endians.Big; // TODO: 初始化为适当的值         
            actual = ByteConverter.ToBytes(value, endian).Reverse().ToArray();
            AssertEqual(expected, actual);
        }

        /// <summary>
        ///ToBytes 的测试
        ///</summary>
        [TestMethod()]
        public void ToBytesTest1()
        {
            ulong value = 5; // TODO: 初始化为适当的值
            Endians endian = Endians.Little; // TODO: 初始化为适当的值
            byte[] expected = BitConverter.GetBytes(value); // TODO: 初始化为适当的值
            byte[] actual;
            actual = ByteConverter.ToBytes(value, endian);
            AssertEqual(expected, actual);

            endian = Endians.Big; // TODO: 初始化为适当的值          
            actual = ByteConverter.ToBytes(value, endian).Reverse().ToArray();
            AssertEqual(expected, actual);
        }

        /// <summary>
        ///ToBytes 的测试
        ///</summary>
        [TestMethod()]
        public void ToBytesTest2()
        {
            int value = 5; // TODO: 初始化为适当的值
            Endians endian = Endians.Little; // TODO: 初始化为适当的值
            byte[] expected = BitConverter.GetBytes(value); // TODO: 初始化为适当的值
            byte[] actual;
            actual = ByteConverter.ToBytes(value, endian);
            AssertEqual(expected, actual);


            endian = Endians.Big; // TODO: 初始化为适当的值          
            actual = ByteConverter.ToBytes(value, endian).Reverse().ToArray();
            AssertEqual(expected, actual);
        }

        /// <summary>
        ///ToBytes 的测试
        ///</summary>
        [TestMethod()]
        public void ToBytesTest3()
        {
            uint value = 5; // TODO: 初始化为适当的值
            Endians endian = Endians.Little; // TODO: 初始化为适当的值
            byte[] expected = BitConverter.GetBytes(value); // TODO: 初始化为适当的值
            byte[] actual;
            actual = ByteConverter.ToBytes(value, endian);
            AssertEqual(expected, actual);

            endian = Endians.Big; // TODO: 初始化为适当的值          
            actual = ByteConverter.ToBytes(value, endian).Reverse().ToArray();
            AssertEqual(expected, actual);
        }

        /// <summary>
        ///ToBytes 的测试
        ///</summary>
        [TestMethod()]
        public void ToBytesTest4()
        {
            short value = 5; // TODO: 初始化为适当的值
            Endians endian = Endians.Little; // TODO: 初始化为适当的值
            byte[] expected = BitConverter.GetBytes(value); // TODO: 初始化为适当的值
            byte[] actual;
            actual = ByteConverter.ToBytes(value, endian);
            AssertEqual(expected, actual);

            endian = Endians.Big; // TODO: 初始化为适当的值          
            actual = ByteConverter.ToBytes(value, endian).Reverse().ToArray();
            AssertEqual(expected, actual);
        }

        /// <summary>
        ///ToBytes 的测试
        ///</summary>
        [TestMethod()]
        public void ToBytesTest5()
        {
            ushort value = 66; // TODO: 初始化为适当的值
            Endians endian = Endians.Little; // TODO: 初始化为适当的值
            byte[] expected = BitConverter.GetBytes(value); // TODO: 初始化为适当的值
            byte[] actual;
            actual = ByteConverter.ToBytes(value, endian);
            AssertEqual(expected, actual);

            endian = Endians.Big; // TODO: 初始化为适当的值          
            actual = ByteConverter.ToBytes(value, endian).Reverse().ToArray();
            AssertEqual(expected, actual);
        }

        /// <summary>
        ///ToInt16 的测试
        ///</summary>
        [TestMethod()]
        public void ToInt16Test()
        {
            byte[] bytes = BitConverter.GetBytes((short)5); // TODO: 初始化为适当的值
            int startIndex = 0; // TODO: 初始化为适当的值
            Endians endian = Endians.Little; // TODO: 初始化为适当的值
            short expected = 5; // TODO: 初始化为适当的值
            short actual;
            actual = ByteConverter.ToInt16(bytes, startIndex, endian);
            Assert.AreEqual(expected, actual);

            short actual2 = ByteConverter.ToInt16(bytes, startIndex, Endians.Big);
            Assert.AreEqual(IPAddress.HostToNetworkOrder(expected), actual2);
        }

        /// <summary>
        ///ToInt32 的测试
        ///</summary>
        [TestMethod()]
        public void ToInt32Test()
        {
            byte[] bytes = BitConverter.GetBytes(5); // TODO: 初始化为适当的值
            int startIndex = 0; // TODO: 初始化为适当的值
            Endians endian = Endians.Little; // TODO: 初始化为适当的值
            int expected = 5; // TODO: 初始化为适当的值
            int actual;
            actual = ByteConverter.ToInt32(bytes, startIndex, endian);
            Assert.AreEqual(expected, actual);

            var actual2 = ByteConverter.ToInt32(bytes, startIndex, Endians.Big);
            Assert.AreEqual(IPAddress.HostToNetworkOrder(expected), actual2);
        }

        /// <summary>
        ///ToInt64 的测试
        ///</summary>
        [TestMethod()]
        public void ToInt64Test()
        {
            byte[] bytes = BitConverter.GetBytes(long.MaxValue); // TODO: 初始化为适当的值
            int startIndex = 0; // TODO: 初始化为适当的值
            Endians endian = Endians.Little; // TODO: 初始化为适当的值
            long expected = long.MaxValue; // TODO: 初始化为适当的值
            long actual;
            actual = ByteConverter.ToInt64(bytes, startIndex, endian);
            Assert.AreEqual(expected, actual);


            var actual2 = ByteConverter.ToInt64(bytes, startIndex, Endians.Big);
            Assert.AreEqual(IPAddress.HostToNetworkOrder(expected), actual2);
        }

        /// <summary>
        ///ToUInt16 的测试
        ///</summary>
        [TestMethod()]
        public void ToUInt16Test()
        {
            byte[] bytes = BitConverter.GetBytes(UInt16.MaxValue); // TODO: 初始化为适当的值
            int startIndex = 0; // TODO: 初始化为适当的值
            Endians endian = Endians.Little; // TODO: 初始化为适当的值
            ushort expected = UInt16.MaxValue; // TODO: 初始化为适当的值
            ushort actual;
            actual = ByteConverter.ToUInt16(bytes, startIndex, endian);
            Assert.AreEqual(expected, actual);


            var actual2 = (short)ByteConverter.ToUInt16(bytes, startIndex, Endians.Big);
            Assert.AreEqual(IPAddress.HostToNetworkOrder((short)expected), actual2);
        }

        /// <summary>
        ///ToUInt32 的测试
        ///</summary>
        [TestMethod()]
        public void ToUInt32Test()
        {
            byte[] bytes = BitConverter.GetBytes(uint.MaxValue); // TODO: 初始化为适当的值
            int startIndex = 0; // TODO: 初始化为适当的值
            Endians endian = Endians.Little; // TODO: 初始化为适当的值
            uint expected = uint.MaxValue; // TODO: 初始化为适当的值
            uint actual;
            actual = ByteConverter.ToUInt32(bytes, startIndex, endian);
            Assert.AreEqual(expected, actual);

            var actual2 = (int)ByteConverter.ToUInt32(bytes, startIndex, Endians.Big);
            Assert.AreEqual(IPAddress.HostToNetworkOrder((int)expected), actual2);
        }

        /// <summary>
        ///ToUInt64 的测试
        ///</summary>
        [TestMethod()]
        public void ToUInt64Test()
        {
            byte[] bytes = BitConverter.GetBytes(ulong.MaxValue); // TODO: 初始化为适当的值
            int startIndex = 0; // TODO: 初始化为适当的值
            Endians endian =  Endians.Little ; // TODO: 初始化为适当的值
            ulong expected = ulong.MaxValue; // TODO: 初始化为适当的值
            ulong actual;
            actual = ByteConverter.ToUInt64(bytes, startIndex, endian);
            Assert.AreEqual(expected, actual);

            var actual2 = (long)ByteConverter.ToUInt64(bytes, startIndex, Endians.Big);
            Assert.AreEqual(IPAddress.HostToNetworkOrder((long)expected), actual2);
        }
    }
}
