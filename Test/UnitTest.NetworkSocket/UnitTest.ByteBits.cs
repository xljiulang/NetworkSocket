using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkSocket;
using System.Collections.Generic;

namespace UnitTest.NetworkSocket
{
    [TestClass]
    public class UnitTestByteBits
    {
        [TestMethod]
        public void Test_implicits()
        {
            var result = true;
            try
            {
                for (var i = byte.MinValue; i <= byte.MinValue; i++)
                {
                    ByteBits b = i;
                }
            }
            catch (Exception)
            {
                result = false;
            }
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_Equals()
        {
            ByteBits b1 = 5;
            ByteBits b2 = 5;
            ByteBits b3 = 7;

            Assert.IsTrue(b1.Equals(b2) && !b2.Equals(b3));
        }

        [TestMethod]
        public void Test_Compare()
        {
            ByteBits b1 = 6;
            ByteBits b2 = 5;
            var list = new List<ByteBits> { b1, b2 };
            list.Sort();

            Assert.IsTrue(list[0] == b2);
        }

        [TestMethod]
        public void Test_Take()
        {
            ByteBits b1 = 2;
            Assert.IsTrue(b1.Take(7, 1) == 0);
        }

        [TestMethod]
        public void Test_Index()
        {
            ByteBits b1 = byte.MaxValue;
            for (var i = 0; i < 8; i++)
            {
                Assert.AreEqual(b1[i], true);
            }

            try
            {
                var bit9 = b1[8];
                Assert.IsTrue(false);
            }
            catch (Exception)
            {
            }
        }
    }
}
