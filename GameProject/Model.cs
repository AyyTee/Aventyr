using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using Poly2Tri;
using System.Diagnostics;
using System;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace Game
{
    /// <summary>
    /// An object made up of vertices
    /// </summary>
    [DataContract]
    public class Model : IDisposable, IVertices
    {
        static object _lockDelete = new object();
        public static object LockDelete { get { return _lockDelete; } }
        [DataMember]
        public Transform3D Transform = new Transform3D();
        int _iboElements;
        public bool IboExists = false;
        /// <summary>If true then gl blending is enabled when rendering this model.</summary>
        [DataMember]
        public bool IsTransparent { get; set; }
        [DataMember]
        public string ShaderName { get; private set; }
        public ShaderProgram Shader
        {
            get
            {
                Debug.Assert(Renderer.Shaders.ContainsKey(ShaderName), "Shader doesn't exist.");
                return Renderer.Shaders[ShaderName]; 
            }
        }
        [DataMember]
        public ITexture Texture;
        [DataMember]
        public Transform2D TransformUv = new Transform2D();
        [DataMember]
        public bool Wireframe = false;

        [DataContract]
        public class Triangle
        {
            public const int NUMBER_OF_VERTICES = 3;
            [DataMember]
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

        [DataMember]
        public List<Vertex> Vertices = new List<Vertex>();
        [DataMember]
        List<Triangle> Triangles = new List<Triangle>();
        #region Constructors
        public Model()
        {
            SetShader("uber");
            InitIbo();
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
        }

        #endregion
        ~Model()
        {
            Dispose();
        }

        private void InitIbo()
        {
            Debug.Assert(IboExists == false, "Model has already been initialized.");
            GL.GenBuffers(1, out _iboElements);
            IboExists = true;
        }

        public int GetIbo()
        {
            if (!IboExists)
            {
                InitIbo();
            }
            return _iboElements;
        }

        public void Dispose()
        {
            if (IboExists)
            {
                lock (LockDelete)
                {
                    if (IboExists)
                    {
                        Controller.iboGarbage.Add(_iboElements);
                        IboExists = false;
                    }
                }
            }
        }

        public void SetShader(string shaderName)
        {
            ShaderProgram a;
            Debug.Assert(Renderer.Shaders.TryGetValue(shaderName, out a));
            ShaderName = shaderName;
        }

        public void SetTexture(ITexture texture)
        {
            Texture = texture;
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

        /// <summary>Adds a Vertex and returns its index.</summary>
        public int AddVertex(Vertex vertex)
        {
            Vertices.Add(vertex);
            return Vertices.Count - 1;
        }

        /// <summary>Adds an array of vertices and returns the index of the first vertex.</summary>
        public int AddVertexRange(Vertex[] vertices)
        {
            Vertices.AddRange(vertices);
            return Vertices.Count - vertices.Length;
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
