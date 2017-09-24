using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Rendering
{
    public interface IRenderer
    {
        List<IRenderWindow> Windows { get; }
        void Render();
    }
}
