using Common.Models;
using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
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
        [DataMember]
        public ImmutableArray<ModelFile> Models { get; private set; }

        public Font DefaultFont => Fonts.Single(item => item.FontData.Info.Face == "LatoRegular");

        public static string ResourcePath => Path.Combine("..", "..", "..", "..", "Build");

        public Resources()
        {
            Textures = new AtlasTexture[0].ToImmutableArray();
            Fonts = new Font[0].ToImmutableArray();
        }

        public Resources(ImmutableArray<AtlasTexture> textures, ImmutableArray<Font> fonts, ImmutableArray<ModelFile> models)
        {
            Textures = textures;
            Fonts = fonts;
            Models = models;
        }
    }
}
