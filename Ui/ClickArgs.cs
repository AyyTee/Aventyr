using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Elements;

namespace Ui
{
    public delegate void OnClickHandler(ClickArgs args);

    public class ClickArgs : ElementArgs
    {
        public bool IsDoubleClick { get; }

        public ClickArgs(bool isDoubleClick, Element parent, Element self)
            : base(parent, self)
        {
            IsDoubleClick = isDoubleClick;
        }
    }
}
