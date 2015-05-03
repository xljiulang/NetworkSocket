using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSocket.WebSocket.Fast;

namespace UnitTest.NetworkSocket.WebSocket.Fast
{
    [TestClass()]
    public class JObjectTest
    {
        class kv
        {
            public string Key;
            public string Value;
        }

        [TestMethod()]
        public void ParseTest()
        {
            var dyObj = new
            {
                key = "key",
                value = new[]
                { 
                    new { key = "key1", value = "value1" },
                    new { key = "key2", value = "value2" } 
                }
            };

            var json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(dyObj);
            var jObj = JObject.Parse(json);
            Assert.IsTrue(jObj.key == "key");

            var value = (JObject)jObj.value;
            Assert.IsTrue(value.IsArray);

            var valueArray = JObject.Cast<kv[]>((object)value);
            Assert.IsTrue(valueArray.Length == 2 && valueArray[0].Value == "value1");
        }
    }
}
