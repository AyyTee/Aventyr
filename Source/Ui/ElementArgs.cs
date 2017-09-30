using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Elements;
using MoreLinq;
using Game;

namespace Ui
{
    public class ElementArgs
    {
        public Element Parent { get; }
        public ElementArgs ParentArgs => Parent.ElementArgs;
        public Element Self { get; }
        public IUiController Controller { get; }
        public int Index => Parent.IndexOf(Self);
        public Element Previous => Parent.ElementAtOrDefault(Index - 1);
        public Element Next => Parent.ElementAtOrDefault(Index + 1);

        public ElementArgs(Element parent, Element self, IUiController controller)
        {
            Parent = parent;
            Self = self;
            Controller = controller;
        }
    }
}
