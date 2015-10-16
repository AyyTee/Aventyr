using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using Poly2Tri;
using System.Diagnostics;
using System;
using System.Xml.Serialization;

namespace Game
{
    /// <summary>
    /// An object made up of vertices
    /// </summary>
    public class Model : IDisposable, IVertices
    {
        public Transform Transform = new Transform();
        [XmlIgnore]
        public int ibo_elements;
        public bool iboExists = true;
        [XmlIgnore]
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

            public Vertex()
            {
            }

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

        public class Triangle
        {
            public const int EDGE_COUNT = 3;
            Vertex[] Vertices = new Vertex[3];
            int[] Indices = new int[3];

            private Triangle()
            {
            }

            public Triangle(Vertex[] vertices, int[] indices)
            {
                Debug.Assert(vertices.Length == 3);
                Vertices = vertices;
                Indices = indices;
            }
            
            public Triangle(Vertex v0, Vertex v1, Vertex v2, int i0, int i1, int i2)
            {
                Vertices[0] = v0;
                Vertices[1] = v1;
                Vertices[2] = v2;
                Indices[0] = i0;
                Indices[1] = i1;
                Indices[2] = i2;
            }

            public Vector3[] GetVerts()
            {
                Vector3[] vList = new Vector3[EDGE_COUNT];
                for (int i = 0; i < Vertices.Length; i++)
                {
                    vList[i] = Vertices[i].Position;
                }
                return vList;
            }
        }

        public List<int> Indices = new List<int>();
        public List<Vertex> Vertices = new List<Vertex>();

        public Model()
        {
            if (Controller.ContextExists)
            {
                Shader = Controller.Shaders["textured"];
                GL.GenBuffers(1, out ibo_elements);
            }
        }

        public Model(Vertex[] vertices, int[] indices)
            : this()
        {
            Vertices.AddRange(vertices);
            Indices.AddRange(indices);
        }

        ~Model()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (iboExists)
            {
                lock ("delete")
                {
                    Controller.iboGarbage.Add(ibo_elements);
                    //GL.DeleteBuffers(1, ref ibo_elements);
                    iboExists = false;
                }
            }
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

        public Vector3[] GetWorldVerts()
        {
            return VectorExt3.Transform(GetVerts(), Transform.GetMatrix());
        }

        public Triangle[] GetTris()
        {
            Debug.Assert(Indices.Count % Triangle.EDGE_COUNT == 0, "Number of indices must be a multiple of 3.");
            Triangle[] tris = new Triangle[Indices.Count/Triangle.EDGE_COUNT];
            for (int i = 0; i < Indices.Count; i += Triangle.EDGE_COUNT)
            {
                int i0 = Indices[i];
                int i1 = Indices[i + 1];
                int i2 = Indices[i + 2];
                tris[i/Triangle.EDGE_COUNT] = new Triangle(Vertices[i0], Vertices[i1], Vertices[i2], i0, i1, i2);
            }
            return tris;
        }
        
        /// <summary>
        /// Returns a convex hull of the model projected onto the z-plane in the world space
        /// </summary>
        /// <returns></returns>
        public Vector2[] GetWorldConvexHull()
        {
            Vector3[] v = GetWorldVerts();
            List<Vector2> vProject = new List<Vector2>();
            for (int i = 0; i < v.Length; i++)
            {
                vProject.Add(new Vector2(v[i].X, v[i].Y));
            }
            return MathExt.ComputeConvexHull(vProject).ToArray();
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
            if (Controller.ContextExists)
            {
                model.SetTexture(Controller.Textures["default.png"]);
            }
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
            if (Controller.ContextExists)
            {
                model.SetTexture(Controller.Textures["default.png"]);
            }
            return model;
        }

        public static Model CreatePolygon(Vector2[] vertices)
        {
            return CreatePolygon(PolygonFactory.CreatePolygon(vertices));
        }

        public static Model CreatePolygon(Polygon polygon)
        {
            Vertex[] verts = new Game.Model.Vertex[polygon.Points.Count];
            List<int> indices = new List<int>();

            for (int i = 0; i < polygon.Points.Count; i++)
            {
                TriangulationPoint p = polygon.Points[i];
                Vector3 v = new Vector3((float)p.X, (float)p.Y, 0);
                float tx = (float)((p.X - polygon.MinX) / (polygon.MaxX - polygon.MinX));
                float ty = (float)((p.Y - polygon.MinY) / (polygon.MaxY - polygon.MinY));
                Vector2 tc = new Vector2(tx, ty);
                verts[i] = new Vertex(v, tc);
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
