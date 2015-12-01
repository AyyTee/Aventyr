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
        public int IboElements;
        public bool IboExists = true;

        public string ShaderName = null;
        public ShaderProgram Shader
        {
            get
            {
                Debug.Assert(Renderer.Shaders.ContainsKey(ShaderName), "Shader doesn't exist.");
                return Renderer.Shaders[ShaderName]; 
            }
        }

        public bool IsTextured = false;
        public int TextureId;
        public Transform2D TransformUv = new Transform2D();
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
            ShaderName = "textured";
            GL.GenBuffers(1, out IboElements);
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
            if (IboExists)
            {
                lock ("delete")
                {
                    Controller.iboGarbage.Add(IboElements);
                    IboExists = false;
                }
            }
        }

        public Model(string shaderName)
        {
            ShaderName = shaderName;
            GL.GenBuffers(1, out IboElements);
        }

        public void SetTexture(int textureID)
        {
            TextureId = textureID;
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
            return Vector3Ext.Transform(GetVerts(), Transform.GetMatrix());
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
    }
}
