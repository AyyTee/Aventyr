using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ui
{
    public delegate void OnClickHandler(ClickArgs args);

    public class ClickArgs : ElementArgs
    {
        public bool IsDoubleClick { get; }

        public ClickArgs(bool isDoubleClick, IElement parent, IElement self)
            : base(parent, self)
        {
            IsDoubleClick = isDoubleClick;
        }
    }
}
