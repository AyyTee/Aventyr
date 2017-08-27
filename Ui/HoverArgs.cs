using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Elements;

namespace Ui
{
    public delegate void OnHoverHandler(HoverArgs args);

    public class HoverArgs : ElementArgs
    {
        public HoverArgs(Element parent, Element self)
            : base(parent, self)
        {
        }
    }
}
