using Game;
using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    public class Controller : IUpdateable
    {
        readonly IVirtualWindow _window;

        public Controller(IVirtualWindow window)
        {
            _window = window;
        }

        public void Render(double timeDelta)
        {
            throw new NotImplementedException();
        }

        public void Update(double timeDelta)
        {
            throw new NotImplementedException();
        }
    }
}
