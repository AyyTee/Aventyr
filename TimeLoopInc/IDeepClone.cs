using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    public interface IDeepClone<T>
    {
        T DeepClone();
    }
}
