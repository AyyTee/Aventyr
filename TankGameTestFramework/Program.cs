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
            Thread Thread = new Thread(new ThreadStart(() => { TankGame.Program.Main(new string[0]); }));
            Thread.Start();

            Thread Thread2 = new Thread(new ThreadStart(() => { TankGame.Program.Main(new string[0]); }));
            Thread2.Start();
        }
    }
}
