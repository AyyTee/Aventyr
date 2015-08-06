using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game
{
    public class Polygon : Model
    {
        private class PolygonSimple
        {
            List<Vector2> Vertices = new List<Vector2>();
            List<PolygonSimple> _children = new List<PolygonSimple>();
            public void AddVertices(Vector2 vertex)
            {
                Vertices.Add(vertex);
            }

            public void AddVertices(Vector2[] vertices)
            {
                Vertices.AddRange(vertices);
            }

            public void AddChild()
            {

            }
        }

        private bool _isValid = true;
        public bool IsValid { get { return _isValid; } }
        private PolygonSimple _root = new PolygonSimple();
        private PolygonSimple Root { get { return _root; } set { _root = value; } }
        
        public PolygonSimple AddChild()
        {
        }
    }
}
