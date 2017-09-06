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
        public bool IsSelected { get; }

        public ModelArgs(bool isSelected, Element parent, Element self) 
            : base(parent, self)
        {
            IsSelected = isSelected;
        }
    }
}
