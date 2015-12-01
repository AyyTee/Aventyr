using OpenTK;
using Poly2Tri;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public static class ModelFactory
    {
        public static Model CreatePlane(Vector2 Scale)
        {
            Model.Vertex[] vertices = new Model.Vertex[] {
                new Model.Vertex(new Vector3(-0.5f * Scale.X, 0.5f * Scale.Y,  0f), new Vector2(0, 1)),
                new Model.Vertex(new Vector3(0.5f * Scale.X, 0.5f * Scale.Y,  0f), new Vector2(1, 1)),
                new Model.Vertex(new Vector3(0.5f * Scale.X, -0.5f * Scale.Y,  0f), new Vector2(1, 0)),
                new Model.Vertex(new Vector3(-0.5f * Scale.X, -0.5f * Scale.Y,  0f), new Vector2(0, 0))
            };

            int[] indices = new int[] {
                0, 2, 1,
                0, 3, 2,
            };
            Model model = new Model(vertices, indices);
            model.SetTexture(Renderer.Textures["default.png"]);
            return model;
        }

        public static Model CreatePlane()
        {
            return CreatePlane(new Vector2(1, 1));
        }

        /*public static Model CreateCircle()
        {
            Model model = new Model();
        }*/

        public static Model CreateCube()
        {
            Model.Vertex[] vertices = new Model.Vertex[] {
                //left
                new Model.Vertex(new Vector3(-0.5f, -0.5f,  -0.5f), new Vector2(1.0f, 0.0f)),
                new Model.Vertex(new Vector3(0.5f, 0.5f,  -0.5f), new Vector2(0.0f, 1.0f)),
                new Model.Vertex(new Vector3(0.5f, -0.5f,  -0.5f), new Vector2(0.0f, 0.0f)),
                new Model.Vertex(new Vector3(-0.5f, 0.5f,  -0.5f), new Vector2(1.0f, 1.0f)),
                //back
                new Model.Vertex(new Vector3(0.5f, -0.5f,  -0.5f), new Vector2(1.0f, 0.0f)),
                new Model.Vertex(new Vector3(0.5f, 0.5f,  -0.5f), new Vector2(1.0f, 1.0f)),
                new Model.Vertex(new Vector3(0.5f, 0.5f,  0.5f), new Vector2(0.0f, 1.0f)),
                new Model.Vertex(new Vector3(0.5f, -0.5f,  0.5f), new Vector2(0.0f, 0.0f)),
                //right
                new Model.Vertex(new Vector3(-0.5f, -0.5f,  0.5f), new Vector2(0.0f, 0.0f)),
                new Model.Vertex(new Vector3(0.5f, -0.5f,  0.5f), new Vector2(1.0f, 0.0f)),
                new Model.Vertex(new Vector3(0.5f, 0.5f,  0.5f), new Vector2(1.0f, 1.0f)),
                new Model.Vertex(new Vector3(-0.5f, 0.5f,  0.5f), new Vector2(0.0f, 1.0f)),
                //top
                new Model.Vertex(new Vector3(0.5f, 0.5f,  -0.5f), new Vector2(1.0f, 0.0f)),
                new Model.Vertex(new Vector3(-0.5f, 0.5f,  -0.5f), new Vector2(1.0f, 1.0f)),
                new Model.Vertex(new Vector3(0.5f, 0.5f,  0.5f), new Vector2(0.0f, 0.0f)),
                new Model.Vertex(new Vector3(-0.5f, 0.5f,  0.5f), new Vector2(0.0f, 1.0f)),
                //front
                new Model.Vertex(new Vector3(-0.5f, -0.5f,  -0.5f), new Vector2(0.0f, 0.0f)), 
                new Model.Vertex(new Vector3(-0.5f, 0.5f,  0.5f), new Vector2(1.0f, 1.0f)), 
                new Model.Vertex(new Vector3(-0.5f, 0.5f,  -0.5f), new Vector2(0.0f, 1.0f)),
                new Model.Vertex(new Vector3(-0.5f, -0.5f,  0.5f), new Vector2(1.0f, 0.0f)),
                //bottom
                new Model.Vertex(new Vector3(-0.5f, -0.5f,  -0.5f), new Vector2(1.0f, 0.0f)), 
                new Model.Vertex(new Vector3(0.5f, -0.5f,  -0.5f), new Vector2(1.0f, 1.0f)),
                new Model.Vertex(new Vector3(0.5f, -0.5f,  0.5f), new Vector2(0.0f, 1.0f)),
                new Model.Vertex(new Vector3(-0.5f, -0.5f,  0.5f), new Vector2(0.0f, 0.0f))
            };

            int[] indices = new int[] {
                //left
                0,1,2,0,3,1,
                //back
                4,5,6,4,6,7,
                //right
                8,9,10,8,10,11,
                //top
                13,14,12,13,15,14,
                //front
                16,17,18,16,19,17,
                //bottom 
                20,21,22,20,22,23
            };
            Model model = new Model(vertices, indices);
            model.SetTexture(Renderer.Textures["default.png"]);
            return model;
        }

        public static Model CreatePolygon(Vector2[] vertices)
        {
            return CreatePolygon(PolygonFactory.CreatePolygon(vertices));
        }

        public static Model CreatePolygon(Polygon polygon)
        {
            Model.Vertex[] verts = new Model.Vertex[polygon.Points.Count];
            List<int> indices = new List<int>();

            for (int i = 0; i < polygon.Points.Count; i++)
            {
                TriangulationPoint p = polygon.Points[i];
                Vector3 v = new Vector3((float)p.X, (float)p.Y, 0);
                float tx = (float)((p.X - polygon.MinX) / (polygon.MaxX - polygon.MinX));
                float ty = (float)((p.Y - polygon.MinY) / (polygon.MaxY - polygon.MinY));
                Vector2 tc = new Vector2(tx, ty);
                verts[i] = new Model.Vertex(v, tc);
            }

            foreach (Poly2Tri.DelaunayTriangle t in polygon.Triangles)
            {
                indices.Add(polygon.IndexOf(t.Points._0));
                indices.Add(polygon.IndexOf(t.Points._1));
                indices.Add(polygon.IndexOf(t.Points._2));
            }

            return new Model(verts, indices.ToArray());
        }

        public static Model CreateLine(Vector2[] vertices)
        {
            Model model = new Model();
            model.Wireframe = true;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = new Vector3(vertices[i].X, vertices[i].Y, 0);
                model.Vertices.Add(new Model.Vertex(v));
                if (i > 0)
                {
                    model.Indices.AddRange(new int[] { i - 1, i, i });
                }
            }
            return model;
        }

        public static Model CreateCircle(Vector3 origin, float radius, int detail)
        {
            Model model = new Model();
            for (int i = 0; i < detail; i++)
            {
                double rad = Math.PI * 2 * i / detail;
                Vector3 pos = new Vector3((float)Math.Cos(rad), (float)Math.Sin(rad), 0) * radius + origin;
                Vector2 textureCoord = new Vector2((float)(1 + Math.Cos(rad) / 2), (float)(1 + Math.Sin(rad) / 2));
                model.Vertices.Add(new Model.Vertex(pos, textureCoord));
            }

            for (int i = 0; i < detail - 1; i++)
            {
                model.Indices.Add(i);
                model.Indices.Add(i + 1);
                model.Indices.Add(detail - 1 - i);
            }
            return model;
        }
    }
}
