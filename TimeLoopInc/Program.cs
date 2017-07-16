using Game;
using Game.Common;
using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
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
            window.OnExit += controller.Exit;
            controller.AddController(new Controller(window));

            controller.Run();
        }
    }
}
