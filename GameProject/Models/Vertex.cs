using System.Diagnostics;
using System.Runtime.Serialization;
using Game.Common;
using Game.Serialization;
using OpenTK;

namespace Game.Models
{
    [DataContract, DebuggerDisplay("Vertex {Position}")]
    public class Vertex : IShallowClone<Vertex>
    {
        [DataMember]
        public readonly Vector3 Position;
        [DataMember]
        public readonly Vector3 Color;
        [DataMember]
        public readonly Vector2 TextureCoord;
        [DataMember]
        public readonly Vector3 Normal;

        public Vertex()
            : this(new Vector3())
        {
        }

        public Vertex(float x, float y, float z = 0)
            : this(new Vector3(x, y, z))
        {
        }

        public Vertex(Vector2 position)
            : this(new Vector3(position))
        {
        }

        public Vertex(Vector3 position)
            : this(position, new Vector2(), new Vector3(), new Vector3())
        {
        }

        public Vertex(Vector3 position, Vector2 textureCoord)
            : this(position, textureCoord, new Vector3(), new Vector3())
        {
        }

        public Vertex(Vector3 position, Vector2 textureCoord, Vector3 color)
            : this(position, textureCoord, color, new Vector3())
        {
        }

        public Vertex(Vector3 position, Vector2 textureCoord, Vector3 color, Vector3 normal)
        {
            Position = position;
            TextureCoord = textureCoord;
            Color = color;
            Normal = normal;
        }

        public Vertex ShallowClone()
        {
            return new Vertex(Position, TextureCoord, Color, Normal);
        }

        public static Vertex Lerp(Vertex v0, Vertex v1, float t, bool normalizeNormals = true)
        {
            Vector3 normal = MathExt.Lerp(v0.Normal, v1.Normal, t);
            if (normalizeNormals && normal.Length > 0)
            {
                normal.Normalize();
            }
            Vertex vNew = new Vertex(
                MathExt.Lerp(v0.Position, v1.Position, t),
                MathExt.Lerp(v0.TextureCoord, v1.TextureCoord, t),
                MathExt.Lerp(v0.Color, v1.Color, t),
                normal
                );
            return vNew;
        }

        public Vertex Transform(Matrix4 transform)
        {
            Vector3 position = Vector3.Transform(Position, transform);
            Vector3 normal = Vector3.Transform(Normal, transform);
            return new Vertex(position, TextureCoord, Color, normal);
        }

        public static bool Equals(Vertex v0, Vertex v1)
        {
            if (((object)v0) == null && ((object)v1) == null)
            {
                return true;
            }
            else if (((object)v0) == null || ((object)v1) == null)
            {
                return false;
            }
            
            if (v0.Position == v1.Position &&
                v0.Normal == v1.Normal &&
                v0.TextureCoord == v1.TextureCoord &&
                v0.Color == v1.Color)
            {
                Debug.Assert(v0.GetHashCode() == v1.GetHashCode());
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if a pair of vertices have the same position, normal, color, and texture coordinate.
        /// </summary>
        public bool Equals(Vertex vertex)
        {
            return Equals(this, vertex);
        }

        public override bool Equals(object vertex)
        {
            if (vertex is Vertex)
            {
                return Equals(this, (Vertex)vertex);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode() ^ 
                TextureCoord.GetHashCode() ^
                Color.GetHashCode() ^
                Normal.GetHashCode();
        }

        public static bool operator ==(Vertex v0, Vertex v1)
        {
            return Equals(v0, v1);
        }

        public static bool operator !=(Vertex v0, Vertex v1)
        {
            return !Equals(v0, v1);
        }
    }
}
