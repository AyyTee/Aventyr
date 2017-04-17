using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class AssetPaths
    {
        public static string FontFolder => Path.Combine(new[] { "Assets", "Fonts" });
        public static string ShaderFolder => Path.Combine(new[] { "Assets", "Shaders" });
        public static string TextureFolder => Path.Combine(new[] { "Assets", "Textures" });
        public static string SoundFolder => Path.Combine(new[] { "Assets", "Sounds" });
    }
}
