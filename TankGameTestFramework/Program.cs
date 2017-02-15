using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TankGame;

namespace TankGameTestFramework
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Run(true);

            using (Game.Window window = new Game.Window())
            {
                window.Controller = new TankGame.Controller(window, args);
                window.Run(Game.Controller.StepsPerSecond, Game.Controller.DrawsPerSecond);
            }
        }

        static void Run(bool useThreads)
        {
            const int serverPort = 45619;
            const int clientCount = 2;

            if (useThreads)
            {
                var server = new Thread(() =>
                {
                    TankGame.Program.Main(new[] {
                        "server", serverPort.ToString()
                    });
                });
                server.Name = "Server";
                server.Start();

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
            else
            {
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                var server = new Process();
                server.StartInfo.FileName = "TankGame.exe";
                server.StartInfo.WorkingDirectory = path;
                server.StartInfo.Arguments = "server " + serverPort;
                server.Start();

                var clients = new Process[clientCount];
                for (int i = 0; i < clientCount; i++)
                {
                    Thread.Sleep(1000);
                    int clientPort = serverPort + 1 + i;
                    clients[i] = new Process();
                    clients[i].StartInfo.FileName = "TankGame.exe";
                    clients[i].StartInfo.WorkingDirectory = path;
                    clients[i].StartInfo.Arguments = "client " + clientPort + " " + serverPort;
                    clients[i].Start();
                }

                Console.WriteLine("Press enter to close all.");
                Console.ReadLine();
                server.Kill();
                foreach (Process process in clients)
                {
                    process.Kill();
                }
            }
        }
    }
}
