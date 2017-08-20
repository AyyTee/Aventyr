using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ui
{
    public class ModelArgs : ElementArgs
    {
        public bool IsSelected { get; }

        public ModelArgs(bool isSelected, IElement parent, IElement self) 
            : base(parent, self)
        {
            IsSelected = isSelected;
        }
    }
}
