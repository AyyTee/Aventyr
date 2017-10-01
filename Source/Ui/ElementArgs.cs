using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Elements;
using MoreLinq;
using Game;
using Game.Common;

namespace Ui
{
    public class ElementArgs
    {
        public Element Parent { get; }
        public ElementArgs ParentArgs => Parent.ElementArgs;
        public Element Self { get; }
        public IUiController Controller { get; }
        public int Index {
            get
            {
                var index = Parent.IndexOf(Self);
                DebugEx.Assert(index != -1, "Element must exist in parent's child list.");
                return index;
            }
        }
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
