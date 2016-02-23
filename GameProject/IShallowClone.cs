using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface IShallowClone<T> where T : IShallowClone<T>
    {
        T ShallowClone();
    }
}
