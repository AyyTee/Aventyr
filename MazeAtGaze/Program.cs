using EyeXFramework;
using Game;
using Game.Common;
using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tobii.EyeX;

namespace MazeAtGaze
{
    class Program
    {
        static void Main(string[] args)
        {
            var controller = new ResourceController(new Vector2i(1000, 800));
            var window = new VirtualWindow(controller)
            {
                CanvasSize = controller.ClientSize
            };
            using (var host = new EyeXHost())
            {
                controller.AddController(new Controller(window, host));

                controller.Run();
            }
        }
    }
}
