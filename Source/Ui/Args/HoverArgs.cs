using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Elements;

namespace Ui.Args
{
    public class HoverArgs : ElementArgs
    {
        /// <summary>
        /// True if the mouse entered. False if it exited.
        /// </summary>
        public bool MouseEntered { get; }

        public HoverArgs(IElement element, bool mouseEntered)
            : base(element.ElementArgs.Parent, element.ElementArgs.Self)
        {
            MouseEntered = mouseEntered;
        }
    }
}
