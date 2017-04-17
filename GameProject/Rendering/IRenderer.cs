using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Rendering
{
    public interface IRenderer
    {
        List<IVirtualWindow> Windows { get; }
        void Render();
    }
}
