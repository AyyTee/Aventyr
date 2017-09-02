using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Game.Rendering
{
    [DataContract]
    public class AtlasTexture : ITexture
    {
        [DataMember]
        public TextureFile Texture { get; private set; }
        [DataMember]
        public Vector2i Size { get; private set; }
        [DataMember]
        public Vector2i Position { get; private set; }
        [DataMember]
        public bool IsTransparent { get; private set; }
        [DataMember]
        public string Name { get; private set; }
        public int Id => Texture.Id;

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
