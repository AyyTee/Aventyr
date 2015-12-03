using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class Vertex
    {
        public Vector3 Position = new Vector3();
        public Vector3 Color = new Vector3();
        public Vector2 TextureCoord = new Vector2();
        public Vector3 Normal = new Vector3();

        public Vertex()
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
    }
}
