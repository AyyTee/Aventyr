using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TankGameTestFramework
{
    class Program
    {
        static void Main(string[] args)
        {
            int serverPort = 45619;
            Thread server = new Thread(new ThreadStart(() =>
            {
                TankGame.Program.Main(new string[] {
                    "server", serverPort.ToString()
                });
            }));
            server.Name = "Server";
            server.Start();

            

            int clientCount = 2;
            for (int i = 0; i < clientCount; i++)
            {
                Thread.Sleep(1000);
                int clientPort = serverPort + 1 + i;
                Thread client = new Thread(new ThreadStart(() =>
                {
                    TankGame.Program.Main(new string[] {
                        "client", clientPort.ToString(), serverPort.ToString()
                    });
                }));
                client.Name = "Client " + (i + 1);
                client.Start();
            }
        }
    }
}
