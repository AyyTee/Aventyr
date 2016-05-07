using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface IWall : IPolygon
    {
        IList<Vector2> GetWorldVertices();
    }
}
