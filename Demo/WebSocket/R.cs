using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RazorEngine;
using System.Reflection;
using System.IO;
namespace WebSocket
{
    public class R
    {
        public string RR()
        {

            Stream stream = this.GetType().Assembly.GetManifestResourceStream("WebSocket.templte.cs");
            using (var streamR = new StreamReader(stream))
            {
                var templ = streamR.ReadToEnd();
                var x= Razor.Parse(templ, new { Name = 33 });
                return x;
            }
        }
    }
}
