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
        static object _deleteLock = new object();
        public static object LockDelete { get { return _deleteLock; } }
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
        public Texture Texture;
        public Transform2D TransformUv = new Transform2D();
        public bool Wireframe = false;

        public class Triangle
        {
            public const int NUMBER_OF_VERTICES = 3;
            public int[] Indices = new int[NUMBER_OF_VERTICES];

            private Triangle()
            {
            }

            public Triangle(int i0, int i1, int i2)
            {
                Indices[0] = i0;
                Indices[1] = i1;
                Indices[2] = i2;
            }

            public Triangle(int[] indices)
            {
                Debug.Assert(indices.Length == NUMBER_OF_VERTICES, "There can only be 3 indices assigned to a Triangle.");
                Indices = indices;
            }
        }

        //public List<int> Indices = new List<int>();
        public List<Vertex> Vertices = new List<Vertex>();
        List<Triangle> Triangles = new List<Triangle>();
        #region constructors
        public Model()
        {
            SetShader("textured");
            GL.GenBuffers(1, out IboElements);
        }

        public Model(Vertex[] vertices)
            : this(vertices, new Triangle[0])
        {
        }

        public Model(Vertex[] vertices, Triangle[] triangles)
            : this()
        {
            Vertices.AddRange(vertices);
            Triangles.AddRange(triangles);
        }

        public Model(string shaderName)
        {
            SetShader(shaderName);
            GL.GenBuffers(1, out IboElements);
        }

        #endregion
        ~Model()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (IboExists)
            {
                lock (LockDelete)
                {
                    Controller.iboGarbage.Add(IboElements);
                    IboExists = false;
                }
            }
        }

        public void SetShader(string shaderName)
        {
            ShaderName = shaderName;
        }

        public void SetTexture(Texture texture)
        {
            Texture = texture;
            IsTextured = true;
        }

        /// <summary>
        /// Replaces all vertex colors with a single uniform color.
        /// </summary>
        public void SetColor(Vector3 color)
        {
            foreach (Vertex v in Vertices)
            {
                v.Color = color;
            }
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

        public void AddTriangle(Triangle triangle)
        {
            Triangles.Add(triangle);
        }

        public void AddTriangles(Triangle[] triangles)
        {
            Triangles.AddRange(triangles);
        }

        public void AddTriangle(int index0, int index1, int index2)
        {
            Triangles.Add(new Triangle(index0, index1, index2));
        }

        public void AddTriangles(int[] indices)
        {
            for (int i = 0; i < indices.Length; i += 3)
            {
                AddTriangle(indices[i], indices[i + 1], indices[i + 2]);
            }
        }

        /// <summary>
        /// Adds a Vertex and returns its Id.
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public int AddVertex(Vertex vertex)
        {
            Vertices.Add(vertex);
            return Vertices.Count - 1;
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

        /// <summary>
        /// Gets a list of Vertex indices.  Each set of 3 indices defines a triangle.
        /// </summary>
        public int[] GetIndices()
        {
            List<int> indices = new List<int>();
            foreach (Triangle t in Triangles)
            {
                indices.AddRange(t.Indices);
            }
            return indices.ToArray();
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
