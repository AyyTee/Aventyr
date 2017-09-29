using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Elements;

namespace Ui
{
    public class ElementArgs
    {
        public Element Parent { get; }
        public Element Self { get; }
        public IUiController Controller { get; }
        public int Index => Parent.ToList().IndexOf(Self);

        public ElementArgs(Element parent, Element self, IUiController controller)
        {
            Parent = parent;
            Self = self;
            Controller = controller;
        }
    }
}
