using Game;
using Game.Common;
using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aventyr
{
    class Program
    {
        static void Main(string[] args)
        {
            var controller = new ResourceController(new Vector2i(400, 300), "Aventyr");
            var window = new VirtualWindow(controller);
            window.OnExit += controller.Exit;
            controller.AddController(new Controller(window));

            controller.Run();
        }
    }
}
