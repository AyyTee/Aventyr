using System.Collections.Generic;
using System.Runtime.Serialization;
using Game.Common;
using Game.Rendering;
using Game.Serialization;
using OpenTK;

namespace Game.Models
{
    /// <summary>
    /// An object made up of vertices
    /// </summary>
    [DataContract]
    public class Model : IShallowClone<Model>
    {
        public static object LockDelete { get; } = new object();

        [DataMember]
        public Transform3 Transform = new Transform3();

        /// <summary>If true then gl blending is enabled when rendering this model.</summary>
        [DataMember]
        public bool IsTransparent { get; set; }
        [DataMember]
        public ITexture Texture;
        [DataMember]
        public Vector4 Color;
        [DataMember]
        public Transform2 TransformUv = new Transform2();
        [DataMember]
        public bool Wireframe;
        [DataMember]
        public IMesh Mesh = new Mesh();
        
        #region Constructors
        public Model()
        {
        }

        public Model(IMesh mesh)
            : this()
        {
            Mesh = mesh;
        }

        #endregion

        public Model ShallowClone()
        {
            return new Model
            {
                Mesh = Mesh,
                Transform = Transform.ShallowClone(),
                Texture = Texture,
                TransformUv = TransformUv.ShallowClone(),
                Wireframe = Wireframe,
                Color = Color,
                IsTransparent = IsTransparent
            };
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
            var val = new Vector3[vertices.Count];
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
            var vProject = new List<Vector2>();
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
            var val = new Vector3[vertices.Count];
            for (int i = 0; i < val.Length; i++)
            {
                val[i] = vertices[i].Color * (1 - Color.W) + new Vector3(Color * Color.W);
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
