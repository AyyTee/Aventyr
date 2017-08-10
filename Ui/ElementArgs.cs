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
        public DateTime Time { get; }

        public ElementArgs(IElement parent, IElement self, DateTime time)
        {
            Parent = parent;
            Self = self;
            Time = time;
        }
    }
}
