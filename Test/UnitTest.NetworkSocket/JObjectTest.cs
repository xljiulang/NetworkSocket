using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSocket.WebSocket.Fast;
using System.Collections;
using NetworkSocket;

namespace UnitTest.NetworkSocket.WebSocket.Fast
{
    [TestClass()]
    public class JObjectTest
    {
        class User
        {
            public int Age { get; set; }
            public string Name { get; set; }
        }

        [TestMethod()]
        public void ParseTest()
        {
            var model = new
            {
                flag = "test",
                datas = new[]
                { 
                    new { age = "10", name = "张" },
                    new { age ="11", name = "陈" } 
                }
            };
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var json = serializer.Serialize(model);
            var jObject = JObject.Parse(json);
            Assert.IsTrue(jObject.Flag == "test");

            var datas = jObject.Datas as IList;
            Assert.IsTrue(datas != null && datas.Count == 2);
            
            var user = (User)jObject.Datas[0];
            Assert.IsTrue(user != null && user.Name == "张");
        }
    }
}
