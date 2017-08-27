using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Elements;

namespace Ui.Args
{
    public class TypeArgs : ElementArgs
    {
        public IVirtualWindow Window { get; }

        public TypeArgs(IElement element, IVirtualWindow window)
            : base(element.ElementArgs.Parent, element.ElementArgs.Self)
        {
            Window = window;
        }
    }
}
