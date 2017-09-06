using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Elements;

namespace Ui.Args
{
    public class ClickArgs : ElementArgs
    {
        public bool IsDoubleClick { get; }

        public ClickArgs(IElement element, bool isDoubleClick)
            : base(element.ElementArgs.Parent, element.ElementArgs.Self)
        {
            IsDoubleClick = isDoubleClick;
        }
    }
}
