using NetworkSocket;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTest.NetworkSocket
{


    /// <summary>
    ///这是 ByteBuilderTest 的测试类，旨在
    ///包含所有 ByteBuilderTest 单元测试
    ///</summary>
    [TestClass()]
    public class ByteBuilderTest
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
        ///ByteBuilder 构造函数 的测试
        ///</summary>
        [TestMethod()]
        public void ByteBuilderConstructorTest()
        {
            ByteBuilder target = new ByteBuilder();
            Assert.IsTrue(target.Endian == Endians.Big);
        }

        /// <summary>
        ///ByteBuilder 构造函数 的测试
        ///</summary>
        [TestMethod()]
        public void ByteBuilderConstructorTest1()
        {
            Endians endian = new Endians(); // TODO: 初始化为适当的值
            ByteBuilder target = new ByteBuilder(endian);
            Assert.IsTrue(target.Endian == endian && target.Capacity == 1024);
        }

        /// <summary>
        ///ByteBuilder 构造函数 的测试
        ///</summary>
        [TestMethod()]
        public void ByteBuilderConstructorTest2()
        {
            Endians endian = new Endians(); // TODO: 初始化为适当的值
            int capacity = 4; // TODO: 初始化为适当的值
            ByteBuilder target = new ByteBuilder(endian, capacity);
            Assert.IsTrue(target.Endian == endian && target.Capacity == capacity);
        }

        /// <summary>
        ///ByteBuilder 构造函数 的测试
        ///</summary>
        [TestMethod()]
        public void ByteBuilderConstructorTest3()
        {
            Endians endian = new Endians(); // TODO: 初始化为适当的值
            byte[] buffer = new byte[4]; // TODO: 初始化为适当的值
            ByteBuilder target = new ByteBuilder(endian, buffer);
            Assert.IsTrue(target.Endian == endian && target.Capacity == buffer.Length && target.Length == buffer.Length);
        }

        /// <summary>
        ///Add 的测试
        ///</summary>
        [TestMethod()]
        public void AddTest()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            bool value = true; // TODO: 初始化为适当的值
            target.Add(value);
            Assert.IsTrue(target[0] == 1);
        }

        /// <summary>
        ///Add 的测试
        ///</summary>
        [TestMethod()]
        public void AddTest1()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            byte value = 3; // TODO: 初始化为适当的值
            target.Add(value);
            Assert.IsTrue(target[0] == value);
        }

        /// <summary>
        ///Add 的测试
        ///</summary>
        [TestMethod()]
        public void AddTest2()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            short value = 60; // TODO: 初始化为适当的值
            target.Add(value);
            Assert.IsTrue(target.ReadInt16() == value);
        }

        /// <summary>
        ///Add 的测试
        ///</summary>
        [TestMethod()]
        public void AddTest3()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            ushort value = 0; // TODO: 初始化为适当的值
            target.Add(value);
            Assert.IsTrue(target.ReadUInt16() == value);
        }

        /// <summary>
        ///Add 的测试
        ///</summary>
        [TestMethod()]
        public void AddTest4()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            int value = 66; // TODO: 初始化为适当的值
            target.Add(value);
            Assert.IsTrue(target.ReadInt32() == value);
        }

        /// <summary>
        ///Add 的测试
        ///</summary>
        [TestMethod()]
        public void AddTest5()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            uint value = 88; // TODO: 初始化为适当的值
            target.Add(value);
            Assert.IsTrue(target.ReadUInt32() == value);
        }

        /// <summary>
        ///Add 的测试
        ///</summary>
        [TestMethod()]
        public void AddTest6()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            long value = 99; // TODO: 初始化为适当的值
            target.Add(value);
            Assert.IsTrue(target.ReadInt64() == value);
        }

        /// <summary>
        ///Add 的测试
        ///</summary>
        [TestMethod()]
        public void AddTest7()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            ulong value = 99; // TODO: 初始化为适当的值
            target.Add(value);
            Assert.IsTrue(target.ReadUInt64() == value);
        }

        /// <summary>
        ///Add 的测试
        ///</summary>
        [TestMethod()]
        public void AddTest8()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            byte[] value = new byte[] { 1 }; // TODO: 初始化为适当的值
            target.Add(value);
            Assert.IsTrue(target[0] == value[0] && target.Length == 1);
        }

        /// <summary>
        ///Add 的测试
        ///</summary>
        [TestMethod()]
        public void AddTest9()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            byte[] value = new byte[] { 1, 2 }; // TODO: 初始化为适当的值
            int index = 1; // TODO: 初始化为适当的值
            int length = 1; // TODO: 初始化为适当的值
            target.Add(value, index, length);
            Assert.IsTrue(target[0] == value[1] && target.Length == 1);
        }

        /// <summary>
        ///Clear 的测试
        ///</summary>
        [TestMethod()]
        public void ClearTest()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            target.Clear();
            Assert.IsTrue(target.Length == 0);
        }

        /// <summary>
        ///Clear 的测试
        ///</summary>
        [TestMethod()]
        public void ClearTest1()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            target.Add(new byte[] { 1, 2 });
            int length = 1; // TODO: 初始化为适当的值
            target.Clear(length);
            Assert.IsTrue(target.Length == 1 && target[0] == 2);
        }

        /// <summary>
        ///CopyTo 的测试
        ///</summary>
        [TestMethod()]
        public void CopyToTest()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            byte[] destArray = new byte[3]; // TODO: 初始化为适当的值
            int index = 0; // TODO: 初始化为适当的值
            int length = 3; // TODO: 初始化为适当的值
            target.CopyTo(destArray, index, length);

            foreach (var b in destArray)
            {
                Assert.IsTrue(b == 0);
            }
        }

        /// <summary>
        ///CutTo 的测试
        ///</summary>
        [TestMethod()]
        public void CutToTest()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            target.Add(int.MaxValue);
            byte[] destArray = new byte[4]; // TODO: 初始化为适当的值
            int index = 0; // TODO: 初始化为适当的值
            int length = 4; // TODO: 初始化为适当的值
            target.CutTo(destArray, index, length);
            Assert.IsTrue(target.Length == 0);
            Assert.IsTrue(int.MaxValue == ByteConverter.ToInt32(destArray, index, Endians.Big));

        }

        /// <summary>
        ///ReadArray 的测试
        ///</summary>
        [TestMethod()]
        public void ReadArrayTest()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            target.Add(new byte[] { 1, 2, 3 });
            int length = 2; // TODO: 初始化为适当的值
            byte[] expected = new byte[] { 1, 2 }; // TODO: 初始化为适当的值
            byte[] actual;
            actual = target.ReadArray(length);
            Assert.AreEqual(expected[0], actual[0]);
        }

        /// <summary>
        ///ReadArray 的测试
        ///</summary>
        [TestMethod()]
        public void ReadArrayTest1()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            target.Add(new byte[] { 1, 2, 3 });
            byte[] expected = new byte[] { 1, 2, 3 }; // TODO: 初始化为适当的值
            byte[] actual;
            actual = target.ReadArray();
            Assert.AreEqual(expected.Length, actual.Length);
        }

        /// <summary>
        ///ReadBoolean 的测试
        ///</summary>
        [TestMethod()]
        public void ReadBooleanTest()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            bool expected = false; // TODO: 初始化为适当的值
            bool actual;
            actual = target.ReadBoolean();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("验证此测试方法的正确性。");
        }

        /// <summary>
        ///ReadByte 的测试
        ///</summary>
        [TestMethod()]
        public void ReadByteTest()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            target.Add((byte)100);
            byte expected = 100; // TODO: 初始化为适当的值
            byte actual;
            actual = target.ReadByte();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///ReadInt16 的测试
        ///</summary>
        [TestMethod()]
        public void ReadInt16Test()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            short expected = 0; // TODO: 初始化为适当的值
            short actual;
            actual = target.ReadInt16();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("验证此测试方法的正确性。");
        }

        /// <summary>
        ///ReadInt32 的测试
        ///</summary>
        [TestMethod()]
        public void ReadInt32Test()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            int expected = 0; // TODO: 初始化为适当的值
            int actual;
            actual = target.ReadInt32();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("验证此测试方法的正确性。");
        }

        /// <summary>
        ///ReadInt64 的测试
        ///</summary>
        [TestMethod()]
        public void ReadInt64Test()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            long expected = 0; // TODO: 初始化为适当的值
            long actual;
            actual = target.ReadInt64();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("验证此测试方法的正确性。");
        }

        /// <summary>
        ///ReadUInt16 的测试
        ///</summary>
        [TestMethod()]
        public void ReadUInt16Test()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            uint expected = 0; // TODO: 初始化为适当的值
            uint actual;
            actual = target.ReadUInt16();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("验证此测试方法的正确性。");
        }

        /// <summary>
        ///ReadUInt32 的测试
        ///</summary>
        [TestMethod()]
        public void ReadUInt32Test()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            uint expected = 0; // TODO: 初始化为适当的值
            uint actual;
            actual = target.ReadUInt32();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("验证此测试方法的正确性。");
        }

        /// <summary>
        ///ReadUInt64 的测试
        ///</summary>
        [TestMethod()]
        public void ReadUInt64Test()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            ulong expected = 0; // TODO: 初始化为适当的值
            ulong actual;
            actual = target.ReadUInt64();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("验证此测试方法的正确性。");
        }

        /// <summary>
        ///ToArray 的测试
        ///</summary>
        [TestMethod()]
        public void ToArrayTest()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            target.Add(new byte[] { 1, 2, 3 });
            byte[] expected = new byte[] { 1, 2, 3 }; // TODO: 初始化为适当的值
            byte[] actual;
            actual = target.ToArray();
            Assert.AreEqual(expected.Length, actual.Length);
        }

        /// <summary>
        ///ToArray 的测试
        ///</summary>
        [TestMethod()]
        public void ToArrayTest1()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            target.Add(new byte[] { 1, 2, 3 });
            int index = 1; // TODO: 初始化为适当的值
            byte[] expected = new byte[] { 2, 3 }; // TODO: 初始化为适当的值
            byte[] actual;
            actual = target.ToArray(index);
            Assert.AreEqual(expected[0], actual[0]);
        }

        /// <summary>
        ///ToArray 的测试
        ///</summary>
        [TestMethod()]
        public void ToArrayTest2()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            target.Add(new byte[] { 1, 2, 3 });
            int index = 1; // TODO: 初始化为适当的值
            int length = 1; // TODO: 初始化为适当的值
            byte[] expected = new byte[] { 2 }; // TODO: 初始化为适当的值
            byte[] actual;
            actual = target.ToArray(index, length);
            Assert.AreEqual(expected[0], actual[0]);
            Assert.AreEqual(expected.Length, actual.Length);
        }

        /// <summary>
        ///ToBoolean 的测试
        ///</summary>
        [TestMethod()]
        public void ToBooleanTest()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            target.Add(true);
            int index = 0; // TODO: 初始化为适当的值
            bool expected = true; // TODO: 初始化为适当的值
            bool actual;
            actual = target.ToBoolean(index);
            Assert.AreEqual(expected, actual); ;
        }

        /// <summary>
        ///ToInt16 的测试
        ///</summary>
        [TestMethod()]
        public void ToInt16Test()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            target.Add((short)2);
            int index = 0; // TODO: 初始化为适当的值
            short expected = 2; // TODO: 初始化为适当的值
            short actual;
            actual = target.ToInt16(index);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///ToInt32 的测试
        ///</summary>
        [TestMethod()]
        public void ToInt32Test()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            target.Add(int.MaxValue);
            int index = 0; // TODO: 初始化为适当的值
            int expected = int.MaxValue; // TODO: 初始化为适当的值
            int actual;
            actual = target.ToInt32(index);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///ToInt64 的测试
        ///</summary>
        [TestMethod()]
        public void ToInt64Test()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            target.Add(long.MaxValue);
            int index = 0; // TODO: 初始化为适当的值
            long expected = long.MaxValue; // TODO: 初始化为适当的值
            long actual;
            actual = target.ToInt64(index);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///ToUInt16 的测试
        ///</summary>
        [TestMethod()]
        public void ToUInt16Test()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            target.Add(ushort.MaxValue);
            int index = 0; // TODO: 初始化为适当的值
            uint expected = ushort.MaxValue; // TODO: 初始化为适当的值
            uint actual;
            actual = target.ToUInt16(index);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///ToUInt32 的测试
        ///</summary>
        [TestMethod()]
        public void ToUInt32Test()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            target.Add(uint.MaxValue);
            int index = 0; // TODO: 初始化为适当的值
            uint expected = uint.MaxValue; // TODO: 初始化为适当的值
            uint actual;
            actual = target.ToUInt32(index);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///ToUInt64 的测试
        ///</summary>
        [TestMethod()]
        public void ToUInt64Test()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            target.Add(ulong.MaxValue);
            int index = 0; // TODO: 初始化为适当的值
            ulong expected = ulong.MaxValue; // TODO: 初始化为适当的值
            ulong actual;
            actual = target.ToUInt64(index);
            Assert.AreEqual(expected, actual); ;
        }

        /// <summary>
        ///Item 的测试
        ///</summary>
        [TestMethod()]
        public void ItemTest()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            target.Add(new byte[] { 1, 2 });
            int index = 1; // TODO: 初始化为适当的值
            byte expected = 2; // TODO: 初始化为适当的值
            byte actual;
            target[index] = expected;
            actual = target[index];
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///Position 的测试
        ///</summary>
        [TestMethod()]
        public void PositionTest()
        {
            ByteBuilder target = new ByteBuilder(); // TODO: 初始化为适当的值
            int expected = 0; // TODO: 初始化为适当的值
            int actual;
            target.Position = expected;
            actual = target.Position;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("验证此测试方法的正确性。");
        }
    }
}
