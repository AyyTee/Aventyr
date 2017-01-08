using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TankGameTestFramework
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const int serverPort = 45619;
            var server = new Thread(() =>
            {
                TankGame.Program.Main(new[] {
                    "server", serverPort.ToString()
                });
            });
            server.Name = "Server";
            server.Start();

            int clientCount = 1;
            for (int i = 0; i < clientCount; i++)
            {
                Thread.Sleep(1000);
                int clientPort = serverPort + 1 + i;
                var client = new Thread(() =>
                {
                    TankGame.Program.Main(new[] {
                        "client", clientPort.ToString(), serverPort.ToString()
                    });
                });
                client.Name = "Client " + (i + 1);
                client.Start();
            }
        }
    }
}
