using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Args;

namespace Ui
{
    public interface IClickable : IHoverable, IElement
    {
        ElementAction<ClickArgs> OnClick { get; }
        bool Enabled { get; }
    }
}
