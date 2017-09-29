using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Elements;

namespace Ui
{
    public class ModelArgs : ElementArgs
    {
        public ModelArgs(Element parent, Element self, IUiController controller) 
            : base(parent, self, controller)
        {
        }
    }
}
