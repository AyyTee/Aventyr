using Game;
using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TankGame.Network;

namespace TankGame
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var resourceController = new ResourceController();

            var window = new VirtualWindow(resourceController);
            window.CanvasSize = resourceController.ClientSize;
            resourceController.Controllers.Add(new TankGameController(window));

            resourceController.Run();
        }
    }
}
