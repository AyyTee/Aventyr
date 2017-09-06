using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Args;

namespace Ui
{
    public interface IHoverable : IElement
    {
        ElementAction<HoverArgs> OnHover { get; }
    }
}
