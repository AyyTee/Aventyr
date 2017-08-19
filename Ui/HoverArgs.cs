using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ui
{
    public delegate void OnHoverHandler(HoverArgs args);

    public class HoverArgs : ElementArgs
    {
        public HoverArgs(IElement parent, IElement self)
            : base(parent, self)
        {
        }
    }
}
