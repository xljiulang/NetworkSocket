using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkSocket.Policies;

namespace _843
{
    class Program
    {
        private static FlexPolicyServer flex;

        static void Main(string[] args)
        {


            var owner = NetworkSocket.TcpTable.Snapshot().First(item => item.Port == 4267);


            Console.WriteLine(flex.Port + " is listening...");
            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
