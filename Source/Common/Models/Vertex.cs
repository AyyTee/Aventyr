using System.Diagnostics;
using System.Runtime.Serialization;
using Game.Common;
using Game.Rendering;
using Game.Serialization;
using OpenTK;
using OpenTK.Graphics;
using Equ;

namespace Game.Models
{
    [DataContract, DebuggerDisplay("Vertex {Position}")]
    public class Vertex : MemberwiseEquatable<Vertex>, IShallowClone<Vertex>
    {
        [DataMember]
        public Vector3 Position { get; private set; }
        [DataMember]
        public Color4 Color { get; private set; } = Color4.White;
        [DataMember]
        public Vector2 TextureCoord { get; private set; }
        [DataMember]
        public Vector3 Normal { get; private set; }

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
            : this(position, new Vector2(), new Color4(), new Vector3())
        {
        }

        public Vertex(Vector3 position, Vector2 textureCoord)
            : this(position, textureCoord, Color4.White, new Vector3())
        {
        }

        public Vertex(Vector3 position, Vector2 textureCoord, Color4 color)
            : this(position, textureCoord, color, new Vector3())
        {
        }

        public Vertex(Vector3 position, Vector2 textureCoord, Color4 color, Vector3 normal)
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
            Vector3 normal = v0.Normal.Lerp(v1.Normal, t);
            if (normalizeNormals && normal.Length > 0)
            {
                normal.Normalize();
            }
            Vertex vNew = new Vertex(
                v0.Position.Lerp(v1.Position, t),
                v0.TextureCoord.Lerp(v1.TextureCoord, t),
                v0.Color.Lerp(v1.Color, t),
                normal
                );
            return vNew;
        }

        public Vertex Transform(Matrix4 transform)
        {
            Vector3 position = Vector3Ex.Transform(Position, transform);
            Vector3 normal = Vector3Ex.Transform(Normal, transform);
            return new Vertex(position, TextureCoord, Color, normal);
        }

        public Vertex With(Vector3? position = null, Vector2? textureCoord = null, Color4? color = null, Vector3? normal = null)
        {
            var clone = (Vertex)MemberwiseClone();
            clone.Position = position ?? Position;
            clone.TextureCoord = textureCoord ?? TextureCoord;
            clone.Color = color ?? Color;
            clone.Normal = normal ?? Normal;
            return clone;
        }
    }
}
