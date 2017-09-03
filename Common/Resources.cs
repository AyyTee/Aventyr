using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    [DataContract]
    public partial class Resources
    {
        [DataMember]
        public ImmutableArray<AtlasTexture> Textures { get; private set; }
        [DataMember]
        public ImmutableArray<Font> Fonts { get; private set; }

        public Font DefaultFont => Fonts.Single(item => item.FontData.Info.Face == "LatoRegular");

        public Resources()
        {
            Textures = new AtlasTexture[0].ToImmutableArray();
            Fonts = new Font[0].ToImmutableArray();
        }

        public Resources(ImmutableArray<AtlasTexture> textures, ImmutableArray<Font> fonts)
        {
            Textures = textures;
            Fonts = fonts;
        }
    }
}
