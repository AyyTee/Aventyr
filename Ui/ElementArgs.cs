using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ui
{
    public class ElementArgs
    {
        public IElement Parent { get; }
        public IElement Self { get; }
        public int Index => Parent.ToList().IndexOf(Self);

        public ElementArgs(IElement parent, IElement self)
        {
            Parent = parent;
            Self = self;
        }
    }
}
