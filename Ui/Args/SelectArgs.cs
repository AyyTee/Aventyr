using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Elements;

namespace Ui.Args
{
    public class SelectArgs : ElementArgs
    {
        public bool IsSelected { get; }

        public SelectArgs(IElement element, bool isSelected)
            : base(element.ElementArgs.Parent, element.ElementArgs.Self)
        {
            IsSelected = isSelected;
        }
    }
}
