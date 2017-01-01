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
            using (Game.Window window = new Game.Window())
            {
                window.Controller = new Controller(window, args);
                window.Run(Game.Controller.StepsPerSecond, Game.Controller.DrawsPerSecond);
            }
        }
    }
}
