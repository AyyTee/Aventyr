using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    [DataContract]
    public class Vertex : ILerp<Vertex>
    {
        [DataMember]
        public Vector3 Position;
        [DataMember]
        public Vector3 Color;
        [DataMember]
        public Vector2 TextureCoord;
        [DataMember]
        public Vector3 Normal;

        public Vertex()
            : this(new Vector3())
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

        public Vertex Lerp(Vertex v1, float t)
        {
            return Lerp(this, v1, t);
        }

        public static Vertex Lerp(Vertex v0, Vertex v1, float t)
        {
            Vertex vNew = new Vertex();
            vNew.Position = MathExt.Lerp(v0.Position, v1.Position, t);
            vNew.Normal = MathExt.Lerp(v0.Normal, v1.Normal, t);
            vNew.TextureCoord = MathExt.Lerp(v0.TextureCoord, v1.TextureCoord, t);
            vNew.Color = MathExt.Lerp(v0.Color, v1.Color, t);
            return vNew;
        }

        /// <summary>
        /// Returns true if a vertex has the same position, normal, color, and texture coordinate as this.
        /// </summary>
        public bool Compare(Vertex vertex)
        {
            if (vertex.Position == Position && 
                vertex.Normal == Normal && 
                vertex.TextureCoord == TextureCoord && 
                vertex.Color == Color)
            {
                return true;
            }
            return false;
        }
    }
}
