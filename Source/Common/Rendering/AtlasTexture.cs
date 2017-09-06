using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using OpenTK;

namespace Game.Rendering
{
    [DataContract]
    public class AtlasTexture : ITexture
    {
        [DataMember]
        public TextureFile Texture { get; private set; }
        [DataMember]
        public bool IsTransparent { get; private set; }
        [DataMember]
        public string Name { get; private set; }
        public int Id => Texture.Id;
        public RectangleF UvBounds
        {
            get
            {
                var bounds = new RectangleF(
                    new Vector2(
                        (Position.X + 0.5f) / Texture.Size.X,
                        (Position.Y + 0.5f) / Texture.Size.Y),
                    new Vector2(
                        (Size.X - 1) / (float)Texture.Size.X,
                        (Size.Y - 1) / (float)Texture.Size.Y));
                //DebugEx.Assert((Vector2i)(bounds.Position * (Vector2)Texture.Size) == Position);
                //DebugEx.Assert((Vector2i)(bounds.Size * (Vector2)Texture.Size) == Size);
                return bounds;
            }
        }
        [DataMember]
        public Vector2i Position { get; private set; }
        [DataMember]
        public Vector2i Size { get; private set; }

        public AtlasTexture(TextureFile texture, Vector2i position, Vector2i size, bool isTranparent, string name)
        {
            Texture = texture;
            Position = position;
            Size = size;
            IsTransparent = isTranparent;
            Name = name;
        }
    }
}
