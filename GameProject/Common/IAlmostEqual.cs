using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Common
{
    interface IAlmostEqual<T> where T : IAlmostEqual<T>
    {
        bool AlmostEqual(T comparison, float delta);
        bool AlmostEqualPercent(T comparison, float delta, float percentage);
    }
}
