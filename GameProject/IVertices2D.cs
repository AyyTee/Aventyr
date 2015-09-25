using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    interface IVertices2D
    {
        Vector2[] GetVerts();
        Vector2[] GetWorldVerts();
    }
}
