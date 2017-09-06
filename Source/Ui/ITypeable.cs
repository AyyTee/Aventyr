using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui.Args;

namespace Ui
{
    public interface ITypeable : ISelectable
    {
        ElementAction<TypeArgs> OnTyping { get; }
    }
}
