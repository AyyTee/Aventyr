using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBuilder
{
    public class Paths
    {
        public static string Root => Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../../../.."));
        public static string Tools => Path.Combine(Root, "Tools");
        public static string Build => Path.Combine(Root, "Build");
        public static string Temp => Path.Combine(Root, "Temp");
        public static string TempFonts => Path.Combine(Temp, "Fonts");
        public static string Assets => Path.Combine(Root, "Assets");
        public static string Fonts => Path.Combine(Assets, "Fonts");
        public static string GameSource => Path.Combine(Root, "Source", "Game");

        public static string Blender => Path.Combine(Tools, "Blender");
        public static string BlenderExe => Path.Combine(Blender, Directory.GetDirectories(Blender).First(), "blender.exe");
        public static string Models => Path.Combine(Build, "Models");

        public static string BmFont => Path.Combine(Tools, "bmfont64.exe");
    }
}
