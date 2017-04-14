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
        public static string FontFolder => Path.Combine(new[] { "assets", "fonts" });
        public static string ShaderFolder => Path.Combine(new[] { "assets", "shaders" });
        public static string TextureFolder => Path.Combine(new[] { "assets", "textures" });
        public static string SoundFolder => Path.Combine(new[] { "assets", "sounds" });
    }
}
