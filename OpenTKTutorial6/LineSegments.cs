using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    class LineSegments
    {
        public List<Vector2d[]> List = new List<Vector2d[]>(); 
        public LineSegments()
        {
        }
        public void Add(Vector2d Start, Vector2d End)
        {
            Vector2d[] V = new Vector2d[2];
            V[0] = Start;
            V[1] = End;
            List.Add(V);
        }
        public void AddRange(LineSegments Lines)
        {
            List.AddRange(Lines.List);
        }
        public int Count()
        {
            return List.Count();
        }
        public void DrawDebug()
        {
            GL.Begin(PrimitiveType.Lines);
            for (int i = 0; i < List.Count(); i++)
            {
                GL.Vertex2(List[i][0]);
                GL.Vertex2(List[i][1]);
            }
            
            GL.End();
        }
        public Vector2d[] Last()
        {
            return List[Count() - 1];
        }
    }
}
