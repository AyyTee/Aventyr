using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Runtime.Serialization;

namespace Game
{
    /// <summary>
    /// An object made up of vertices
    /// </summary>
    [DataContract]
    public class Model : IDisposable, IShallowClone<Model>
    {
        static object _lockDelete = new object();
        public static object LockDelete { get { return _lockDelete; } }
        [DataMember]
        public Transform3 Transform = new Transform3();
        int _iboElements;
        public bool IboExists = false;
        /// <summary>If true then gl blending is enabled when rendering this model.</summary>
        [DataMember]
        public bool IsTransparent { get; set; }
        [DataMember]
        public ITexture Texture;
        [DataMember]
        public Vector4 Color = new Vector4();
        [DataMember]
        public Transform2 TransformUv = new Transform2();
        [DataMember]
        public bool Wireframe = false;
        [DataMember]
        public IMesh Mesh = new Mesh();
        
        #region Constructors
        public Model()
        {
            if (Renderer.IsInitialized)
            {
                InitIbo();
            }
        }

        public Model(IMesh mesh)
            : this()
        {
            Mesh = mesh;
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

        public Model ShallowClone()
        {
            Model clone = new Model();
            clone.Mesh = Mesh;
            clone.Transform = Transform.ShallowClone();
            clone.Texture = Texture;
            clone.TransformUv = TransformUv.ShallowClone();
            clone.Wireframe = Wireframe;
            clone.Color = Color;
            clone.IsTransparent = IsTransparent;
            return clone;
        }

        public Model DeepClone()
        {
            Model clone = ShallowClone();
            clone.Mesh = Mesh.ShallowClone();
            return clone;
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
            Color = new Vector4(color, 1);
        }

        public void SetColor(Vector4 color)
        {
            Color = color;
        }

        public Vector3[] GetVerts()
        {
            List<Vertex> vertices = Mesh.GetVertices();
            Vector3[] val = new Vector3[vertices.Count];
            for (int i = 0; i < val.Length; i++)
            {
                val[i] = vertices[i].Position;
            }
            return val;
        }

        public Vector3[] GetWorldVerts()
        {
            return Vector3Ext.Transform(GetVerts(), Transform.GetMatrix());
        }
        
        /// <summary>
        /// Returns a convex hull of the model projected onto the z-plane in the world space
        /// </summary>
        public Vector2[] GetWorldConvexHull()
        {
            Vector3[] v = GetWorldVerts();
            List<Vector2> vProject = new List<Vector2>();
            for (int i = 0; i < v.Length; i++)
            {
                vProject.Add(new Vector2(v[i].X, v[i].Y));
            }
            return MathExt.GetConvexHull(vProject).ToArray();
        }

        /// <summary>
        /// Gets a list of Vertex indices.  Each set of 3 indices defines a triangle.
        /// </summary>
        public int[] GetIndices()
        {
            return Mesh.GetIndices().ToArray();
        }

        public Vector3[] GetColorData()
        {
            List<Vertex> vertices = Mesh.GetVertices();
            Vector3[] val = new Vector3[vertices.Count];
            for (int i = 0; i < val.Length; i++)
            {
                val[i] = vertices[i].Color;
            }
            return val;
        }

        public Vector2[] GetTextureCoords()
        {
            List<Vertex> vertices = Mesh.GetVertices();
            Vector2[] val = new Vector2[vertices.Count];
            
            for (int i = 0; i < val.Length; i++)
            {
                val[i] = vertices[i].TextureCoord;
            }
            return val;
        }
    }
}
