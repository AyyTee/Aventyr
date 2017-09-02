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
        public static string AssetFolder => "Assets";
        public static string FontFolder => Path.Combine(new[] { AssetFolder, "Fonts" });
        public static string ShaderFolder => Path.Combine(new[] { AssetFolder, "Shaders" });
        public static string TextureFolder => Path.Combine(new[] { AssetFolder, "Textures" });
        public static string SoundFolder => Path.Combine(new[] { AssetFolder, "Sounds" });
    }
}
