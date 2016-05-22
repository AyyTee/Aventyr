using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface IPolygon
    {
        IList<Vector2> Vertices { get; }
    }
}
