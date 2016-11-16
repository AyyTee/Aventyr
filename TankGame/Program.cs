using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankGame
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (Game.Window Window = new Game.Window(args))
            {
                Window.controller = new Controller(Window, args);
                Window.Run(Controller.StepsPerSecond, Controller.DrawsPerSecond);
            }
        }
    }
}
