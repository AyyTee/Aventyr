using System.Collections.Generic;
using System.Runtime.Serialization;
using Game.Common;
using Game.Rendering;
using Game.Serialization;
using OpenTK;
using OpenTK.Graphics;

namespace Game.Models
{
    /// <summary>
    /// An object made up of vertices
    /// </summary>
    [DataContract]
    public class Model : IShallowClone<Model>
    {
        [DataMember]
        public Transform3 Transform = new Transform3();
        /// <summary>If true then gl blending is enabled when rendering this model.</summary>
        [DataMember]
        public bool IsTransparent { get; set; }
        [DataMember]
        public ITexture Texture;
        /// <summary>
        /// Color that is blended over top of mesh vertex colors.
        /// </summary>
        [DataMember]
        public Color4 Color;
        /// <summary>
        /// Offset for the mesh uv coordinates.
        /// </summary>
        [DataMember]
        public Transform2 TransformUv = new Transform2();
        [DataMember]
        public bool Wireframe;
        [DataMember]
        public IMesh Mesh = new Mesh();
        
        public Model()
        {
        }

        public Model(IMesh mesh) : this()
        {
            Mesh = mesh;
        }

        public Model ShallowClone() => (Model)MemberwiseClone();

        public Model DeepClone()
        {
            Model clone = ShallowClone();
            clone.Transform = Transform.ShallowClone();
            clone.TransformUv = TransformUv.ShallowClone();
            clone.Mesh = Mesh.ShallowClone();
            return clone;
        }

        public void SetTexture(ITexture texture) => Texture = texture;

        public void SetColor(Color4 color) => Color = color;

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

        public Vector3[] GetWorldVerts() => Vector3Ext.Transform(GetVerts(), Transform.GetMatrix());
        
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
        /// Gets a list of Vertex indices. Each set of 3 indices defines a triangle.
        /// </summary>
        public int[] GetIndices() => Mesh.GetIndices().ToArray();

        public Color4[] GetColorData()
        {
            List<Vertex> vertices = Mesh.GetVertices();
            var val = new Color4[vertices.Count];
            for (int i = 0; i < val.Length; i++)
            {
                val[i] = vertices[i].Color.Lerp(Color, Color.A);
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
