using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitTest.NetworkSocket
{
    [TestClass()]
    public class ConverterTest
    {
        enum MyEnum
        {
            X,
            Y,
            Z
        }

        class User
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        [TestMethod()]
        public void CastTest()
        {
            Assert.AreEqual(-1, Converter.Cast<int>("-1"));
            Assert.AreEqual("-1", Converter.Cast<string>(-1));
            Assert.AreEqual(null, Converter.Cast<string>(null));
            Assert.AreEqual(1, Converter.Cast<int?>(1));
            Assert.AreEqual(null, Converter.Cast<int?>(null));
            Assert.AreEqual(255, Converter.Cast<int>(byte.MaxValue));

            Assert.AreEqual(1.0f, Converter.Cast<float>(1.0d));
            Assert.IsTrue(new int[] { 1, 2 }.SequenceEqual(Converter.Cast<int[]>(new[] { "1", "2" })));
            Assert.IsTrue(new int?[] { 1, 2 }.SequenceEqual(Converter.Cast<int?[]>(new[] { "1", "2" })));

            Assert.AreEqual(MyEnum.Z, Converter.Cast<MyEnum>(MyEnum.Z.GetHashCode()));
            Assert.AreEqual(MyEnum.Z, Converter.Cast<MyEnum>(MyEnum.Z.GetHashCode().ToString()));
            Assert.AreEqual(MyEnum.Z, Converter.Cast<MyEnum>(MyEnum.Z.ToString()));


            var dic = new Dictionary<string, object>();
            dic.Add("name", "陈");
            dic.Add("age", "20");

            var user = Converter.Cast<User>(dic);
            Assert.IsTrue(user != null && user.Name == "陈" && user.Age == 20);

            var user2 = Converter.Cast<User[]>(new[] { dic, dic });
            Assert.IsTrue(user2 != null && user2.Length == 2 && user2[0].Name == "陈" && user2[0].Age == 20);
        }
    }
}
