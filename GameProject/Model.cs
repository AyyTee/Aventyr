using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using Poly2Tri;
using System.Diagnostics;
using System;

namespace Game
{
    /// <summary>
    /// An object made up of vertices
    /// </summary>
    public class Model : IDisposable
    {
        public Transform Transform = new Transform();

        public int ibo_elements;
        public ShaderProgram Shader;

        public bool IsTextured = false;
        public int TextureID;
        public Transform2D TransformUV = new Transform2D();
        public bool Wireframe = false;

        public class Vertex
        {
            public Vector3 Position = new Vector3();
            public Vector3 Color = new Vector3();
            public Vector2 TextureCoord = new Vector2();

            public Vertex(Vector3 position)
            {
                Position = position;
            }

            public Vertex(Vector3 position, Vector2 textureCoord)
            {
                Position = position;
                TextureCoord = textureCoord;
            }

            public Vertex(Vector3 position, Vector2 textureCoord, Vector3 color)
            {
                Position = position;
                TextureCoord = textureCoord;
                Color = color;
            }
        }

        public List<int> Indices = new List<int>();
        public List<Vertex> Vertices = new List<Vertex>();

        public Model()
        {
            Shader = Controller.Shaders["textured"];
            GL.GenBuffers(1, out ibo_elements);
        }

        ~Model()
        {
            Dispose();
        }

        public void Dispose()
        {
            GL.DeleteBuffer(ibo_elements);
        }

        public Model(Vertex[] vertices, int[] indices)
        {
            Vertices.AddRange(vertices);
            Indices.AddRange(indices);
            Shader = Controller.Shaders["textured"];
            GL.GenBuffers(1, out ibo_elements);
        }

        public Model(ShaderProgram shader)
        {
            Shader = shader;
            GL.GenBuffers(1, out ibo_elements);
        }

        public void SetTexture(int textureID)
        {
            TextureID = textureID;
            IsTextured = true;
        }

        public Vector3[] GetVerts()
        {
            Vector3[] val = new Vector3[Vertices.Count];
            for (int i = 0; i < val.Length; i++)
            {
                val[i] = Vertices[i].Position;
            }
            return val;
        }

        public int[] GetIndices()
        {
            return Indices.ToArray();
        }

        public Vector3[] GetColorData()
        {
            Vector3[] val = new Vector3[Vertices.Count];
            for (int i = 0; i < val.Length; i++)
            {
                val[i] = Vertices[i].Color;
            }
            return val;
        }

        public Vector2[] GetTextureCoords()
        {
            Vector2[] val = new Vector2[Vertices.Count];
            
            for (int i = 0; i < val.Length; i++)
            {
                val[i] = Vertices[i].TextureCoord;
            }
            return val;
        }

        public static Model CreatePlane(Vector2 Scale)
        {
            Vertex[] vertices = new Vertex[] {
                new Vertex(new Vector3(-0.5f * Scale.X, 0.5f * Scale.Y,  0f), new Vector2(0, 1)),
                new Vertex(new Vector3(0.5f * Scale.X, 0.5f * Scale.Y,  0f), new Vector2(1, 1)),
                new Vertex(new Vector3(0.5f * Scale.X, -0.5f * Scale.Y,  0f), new Vector2(1, 0)),
                new Vertex(new Vector3(-0.5f * Scale.X, -0.5f * Scale.Y,  0f), new Vector2(0, 0))
            };

            int[] indices = new int[] {
                0, 2, 1,
                0, 3, 2,
            };
            Model model = new Model(vertices, indices);
            model.SetTexture(Controller.textures["default.png"]);
            return model;
        }

        public static Model CreatePlane()
        {
            return CreatePlane(new Vector2(1, 1));
        }

        public static Model CreateCube()
        {
            Vertex[] vertices = new Vertex[] {
                //left
                new Vertex(new Vector3(-0.5f, -0.5f,  -0.5f), new Vector2(1.0f, 0.0f)),
                new Vertex(new Vector3(0.5f, 0.5f,  -0.5f), new Vector2(0.0f, 1.0f)),
                new Vertex(new Vector3(0.5f, -0.5f,  -0.5f), new Vector2(0.0f, 0.0f)),
                new Vertex(new Vector3(-0.5f, 0.5f,  -0.5f), new Vector2(1.0f, 1.0f)),
                //back
                new Vertex(new Vector3(0.5f, -0.5f,  -0.5f), new Vector2(1.0f, 0.0f)),
                new Vertex(new Vector3(0.5f, 0.5f,  -0.5f), new Vector2(1.0f, 1.0f)),
                new Vertex(new Vector3(0.5f, 0.5f,  0.5f), new Vector2(0.0f, 1.0f)),
                new Vertex(new Vector3(0.5f, -0.5f,  0.5f), new Vector2(0.0f, 0.0f)),
                //right
                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f), new Vector2(0.0f, 0.0f)),
                new Vertex(new Vector3(0.5f, -0.5f,  0.5f), new Vector2(1.0f, 0.0f)),
                new Vertex(new Vector3(0.5f, 0.5f,  0.5f), new Vector2(1.0f, 1.0f)),
                new Vertex(new Vector3(-0.5f, 0.5f,  0.5f), new Vector2(0.0f, 1.0f)),
                //top
                new Vertex(new Vector3(0.5f, 0.5f,  -0.5f), new Vector2(1.0f, 0.0f)),
                new Vertex(new Vector3(-0.5f, 0.5f,  -0.5f), new Vector2(1.0f, 1.0f)),
                new Vertex(new Vector3(0.5f, 0.5f,  0.5f), new Vector2(0.0f, 0.0f)),
                new Vertex(new Vector3(-0.5f, 0.5f,  0.5f), new Vector2(0.0f, 1.0f)),
                //front
                new Vertex(new Vector3(-0.5f, -0.5f,  -0.5f), new Vector2(0.0f, 0.0f)), 
                new Vertex(new Vector3(-0.5f, 0.5f,  0.5f), new Vector2(1.0f, 1.0f)), 
                new Vertex(new Vector3(-0.5f, 0.5f,  -0.5f), new Vector2(0.0f, 1.0f)),
                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f), new Vector2(1.0f, 0.0f)),
                //bottom
                new Vertex(new Vector3(-0.5f, -0.5f,  -0.5f), new Vector2(1.0f, 0.0f)), 
                new Vertex(new Vector3(0.5f, -0.5f,  -0.5f), new Vector2(1.0f, 1.0f)),
                new Vertex(new Vector3(0.5f, -0.5f,  0.5f), new Vector2(0.0f, 1.0f)),
                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f), new Vector2(0.0f, 0.0f))
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
            model.SetTexture(Controller.textures["default.png"]);
            return model;
        }

        public static Model CreatePolygon(Poly2Tri.Polygon polygon)
        {
            P2T.Triangulate(polygon);
            Vertex[] vertices = new Vertex[polygon.Points.Count];
            List<int> indices = new List<int>();
            
            for (int i = 0; i < polygon.Points.Count; i++)
            {
                TriangulationPoint p = polygon.Points[i];
                Vector3 v = new Vector3((float)p.X, (float)p.Y, 0);
                float tx = (float)((p.X - polygon.MinX) / (polygon.MaxX - polygon.MinX));
                float ty = (float)((p.Y - polygon.MinY) / (polygon.MaxY - polygon.MinY));
                Vector2 tc = new Vector2(tx, ty);
                vertices[i] = new Vertex(v, tc);
            }
            
            foreach (Poly2Tri.DelaunayTriangle t in polygon.Triangles)
            {
                indices.Add(polygon.IndexOf(t.Points._0));
                indices.Add(polygon.IndexOf(t.Points._1));
                indices.Add(polygon.IndexOf(t.Points._2));
            }

            Model model = new Model(vertices, indices.ToArray());
            return model;
        }

        public static Model CreatePolygon(Vector2[] vertices)
        {
            PolygonPoint[] polygonPoints = new PolygonPoint[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                polygonPoints[i] = new PolygonPoint(vertices[i].X, vertices[i].Y);
            }
            Poly2Tri.Polygon polygon = new Polygon(polygonPoints);
            return CreatePolygon(polygon);
        }

        public static Model CreateLine(Vector2[] vertices)
        {
            Model model = new Model();
            model.Wireframe = true;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = new Vector3(vertices[i].X, vertices[i].Y, 0);
                model.Vertices.Add(new Vertex(v));
                if (i > 0)
                {
                    model.Indices.AddRange(new int[] { i, i - 1, i });
                }
            }
            return model;
        }
    }
}
