using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface ILerp<T>
    {
        T Lerp(T second, float t);
    }
}
