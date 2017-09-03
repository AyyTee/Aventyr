using AtlasTexturePacker.Library;
using Game;
using Game.Common;
using Game.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AssetBuilder
{
    public class Program
    {
        public static string RootPath => Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../../.."));
        public static string ToolsPath => Path.Combine(RootPath, "Tools");
        public static string BuildPath => Path.Combine(RootPath, "Build");
        public static string BuildFontsPath => Path.Combine(BuildPath, "Fonts");
        public static string BuildOutputPath => Path.Combine(BuildPath, "Output");
        public static string AssetsPath => Path.Combine(RootPath, "Assets");
        public static string FontsPath => Path.Combine(AssetsPath, "Fonts");

        public static string BlenderPath => Path.Combine(ToolsPath, "Blender");
        public static string BlenderZipPath => Path.Combine(ToolsPath, "Blender.zip");
        public static string BlenderExePath => Path.Combine(BlenderPath, Directory.GetDirectories(BlenderPath).First(), "blender.exe");
        public static string ModelsPath => Path.Combine(BuildPath, "Models");

        public static string BmFontPath => Path.Combine(ToolsPath, "bmfont64.exe");

        public static volatile int FilesDownloaded = 0;

        public static void Main(string[] args)
        {
            var rootDirectory = new DirectoryInfo(RootPath);
            if (!rootDirectory.GetDirectories().Any(item => item.Name == "Assets"))
            {
                throw new DirectoryNotFoundException("Assets folder is missing from root directory.");
            }

            Clean();
            Initalize();
            FetchTools();
            ExportModels();
            var fontCount = CreateBitmapFonts();
            CreateAtlas(fontCount);

            Console.WriteLine("Build complete!");
            Thread.Sleep(1000);
        }

        public static void Clean()
        {
            Console.WriteLine("Cleaning...");
            if (Directory.Exists(BuildPath))
            {
                Directory.Delete(BuildPath, true);
            }
        }

        public static void Initalize()
        {
            Console.WriteLine("Initializing folder structure...");
            Directory.CreateDirectory(BuildPath);
            Directory.CreateDirectory(ModelsPath);
            Directory.CreateDirectory(ToolsPath);
            Directory.CreateDirectory(BlenderPath);
            Directory.CreateDirectory(BuildFontsPath);
            Directory.CreateDirectory(BuildOutputPath);
        }

        public static void ExportModels()
        {
            Console.WriteLine("Exporting models...\n");
            CommandLine($"{BlenderExePath} Models.blend --background --python BatchExport.py -- {ModelsPath}", AssetsPath);
        }

        class ToolDownload
        {
            public Uri Uri { get; }
            public string Filename { get; }
            public Action DownloadCallback { get; }

            public ToolDownload(Uri uri, string filename, Action downloadCallback = null)
            {
                Uri = uri;
                Filename = filename;
                DownloadCallback = downloadCallback ?? (() => { });
            }
        }

        public static void FetchTools()
        {
            Console.WriteLine("Fetching tools...");
            var toolDownloads = new[]
            {
                new ToolDownload(
                    new Uri("http://download.blender.org/release/Blender2.78/blender-2.78c-windows32.zip"),
                    BlenderZipPath, 
                    () => ZipFile.ExtractToDirectory(BlenderZipPath, BlenderPath)),
                new ToolDownload(
                    new Uri("http://www.angelcode.com/products/bmfont/bmfont64.exe"), 
                    BmFontPath)
            };

            foreach (var toolDownload in toolDownloads)
            {
                if (File.Exists(toolDownload.Filename))
                {
                    Interlocked.Increment(ref FilesDownloaded);
                    continue;
                }

                using (var client = new WebClient())
                {
                    client.DownloadFileCompleted += (sender, e) =>
                    {
                        var tool = (ToolDownload)e.UserState;

                        if (e.Error != null || e.Cancelled)
                        {
                            if (File.Exists(tool.Filename))
                            {
                                File.Delete(tool.Filename);
                            }

                            if (e.Error != null)
                            {
                                throw new Exception($"Failed to download {Path.GetFileName(tool.Filename)}.", e.Error);
                            }
                            else if (e.Cancelled)
                            {
                                throw new Exception($"File download cancelled for {Path.GetFileName(tool.Filename)}.");
                            }
                        }

                        tool.DownloadCallback();

                        Interlocked.Increment(ref FilesDownloaded);
                        Console.WriteLine($"{Path.GetFileName(tool.Filename)} finished downloading.");
                        Console.WriteLine($"{FilesDownloaded} / {toolDownloads.Length} downloaded.");
                    };

                    client.DownloadFileAsync(toolDownload.Uri, toolDownload.Filename, toolDownload);
                }
            }

            while (FilesDownloaded < toolDownloads.Length)
            {
                Thread.Sleep(2000);
            }
        }

        /// <remarks>Original code found here: https://stackoverflow.com/a/32872174 </remarks>
        public static void CommandLine(string command, string workingDirectory = null)
        {
            var cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory;
            cmd.Start();

            cmd.StandardInput.WriteLine(command);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();
            Console.WriteLine(cmd.StandardOutput.ReadToEnd());
        }

        /// <returns>Number of fonts rendered.</returns>
        private static int CreateBitmapFonts()
        {
            var fontConfigs = Directory.GetFiles(FontsPath, "*.bmfc", SearchOption.AllDirectories);
            foreach (var fontConfig in fontConfigs)
            {
                /* -c fontconfig.bmfc : Names the configuration file with the options for generating the font.
                 * -o outputfile.fnt : Names of the output font file.
                 * -t textfile.txt : Optional argument that names a text file. All characters present in the text file will be added to the font.*/
                var outputPath = Path.Combine(BuildFontsPath, Path.GetFileNameWithoutExtension(fontConfig));
                CommandLine($"{BmFontPath} -c {fontConfig} -o {outputPath}");
            }

            return fontConfigs.Length;
        }

        private static void CreateAtlas(int fontCount)
        {
            var bitmaps = new List<BitmapExtended>();

            // Sometimes we read in the fonts before BmFont has had time to finish creating them. Loop here if that happens.
            string[] fontFiles;
            do
            {
                fontFiles = Directory.GetFiles(BuildFontsPath, "*.fnt");
                Thread.Sleep(200);
            } while (fontFiles.Length < fontCount);

            var fonts = fontFiles.Select(item =>
            {
                var font = FontLoader.Load(item);
                font.Info.Face = Path.GetFileNameWithoutExtension(item);
                return font;
            }).ToList();
            var fontPrefix = "font";
            var texturePrefix = "texture";
            for (int i = 0; i < fonts.Count; i++)
            {
                if (fonts[i].Pages.Count > 1)
                {
                    throw new NotImplementedException("Multipage fonts are not supported. Try making the font page larger instead.");
                }

                var fontImagePath = Path.Combine(BuildFontsPath, fonts[i].Pages[0].File);
                DebugEx.Assert(File.Exists(fontImagePath));
                var page = new Bitmap(fontImagePath);

                for (int j = 0; j < fonts[i].Chars.Count; j++)
                {
                    var glyph = GetGlyph(page, fonts[i].Chars[j]);
                    glyph.Name = $"{fontPrefix},{i},{j}";
                    bitmaps.Add(glyph);
                }
            }

            bitmaps.AddRange(Directory
                .GetFiles(Path.Combine(AssetsPath, "Textures"))
                .Select(item =>
                {
                    var bitmap = new BitmapExtended(item);
                    bitmap.Name = $"{texturePrefix},{Path.GetFileNameWithoutExtension(item)},{BitmapIsTransparent(bitmap)}";
                    return bitmap;
                })
                .ToArray());
            var atlas = AtlasCreator.CreateAtlas("Atlas", bitmaps.ToArray());


            var outputAssetsPath = Path.Combine(RootPath, nameof(Game), "Assets");

            var atlasTexture = new TextureFile(Path.Combine("Assets", "Atlas.png"), true);

            DebugEx.Assert(atlas.Length == 1);
            atlas[0].texture.Save(Path.Combine(outputAssetsPath, "Atlas.png"), ImageFormat.Png);

            var textures = new List<AtlasTexture>();

            foreach (var rect in atlas[0].uvRects)
            {
                var split = rect.name.Split(',');
                var x = (int)Math.Round(rect.uvRect.X * atlas[0].texture.Width);
                var y = (int)Math.Round(rect.uvRect.Y * atlas[0].texture.Height);
                if (split[0] == fontPrefix)
                {
                    
                    var fontIndex = int.Parse(split[1]);
                    var charIndex = int.Parse(split[2]);
                    var fontChar = fonts[fontIndex].Chars[charIndex];

                    fontChar.X = x;
                    fontChar.Y = y;
                    fontChar.Width = rect.width - 1;
                    fontChar.Height = rect.height;
                }
                else if (split[0] == texturePrefix)
                {
                    var texture = new AtlasTexture(
                        atlasTexture, 
                        new Vector2i(x, y), 
                        new Vector2i(rect.width, rect.height), 
                        Convert.ToBoolean(split[2]),
                        split[1]);
                    textures.Add(texture);
                }
                else
                {
                    DebugEx.Fail("Atlas textures must have a prefix for identification.");
                }
            }

            foreach (var font in fonts)
            {
                font.Common.ScaleW = atlas[0].texture.Width;
                font.Common.ScaleH = atlas[0].texture.Height;
                font.Pages.Clear();
            }

            var assets = new Resources(
                textures.ToImmutableArray(),
                fonts.Select(item => new Game.Rendering.Font(item, new ITexture[] { atlasTexture })).ToImmutableArray());

            File.WriteAllText(
                Path.Combine(outputAssetsPath, "Assets.xml"), 
                JsonConvert.SerializeObject(assets, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects }));

            CreateAssetCode(assets);
        }

        public static void CreateAssetCode(Resources assets)
        {
            var fonts = "";
            foreach (var font in assets.Fonts)
            {
                fonts += $"        public Font @{font.FontData.Info.Face} => Fonts.Single(item => item.FontData.Info.Face == \"{font.FontData.Info.Face}\");\n";
            }

            var textures = "";
            foreach (var texture in assets.Textures)
            {
                textures += $"        public AtlasTexture @{texture.Name} => Textures.Single(item => item.Name == \"{texture.Name}\");\n";
            }

            var code = 
$@"// This code was autogenerated.

using Game.Rendering;
using System.Linq;

namespace Game
{{
    public partial class {nameof(Resources)}
    {{
{fonts}
{textures}
    }}
}}
";
            File.WriteAllText(Path.Combine(RootPath, nameof(Game), $"{nameof(Resources)}Generated.cs"), code);
        }

        static BitmapExtended GetGlyph(Bitmap page, FontChar fontChar)
        {
            var glyph = new BitmapExtended(fontChar.Width, fontChar.Height);
            for (int y = 0; y < fontChar.Height; y++)
            {
                for (int x = 0; x < fontChar.Width; x++)
                {
                    var color = page.GetPixel(fontChar.X + x, fontChar.Y + y);
                    glyph.SetPixel(x, y, color);
                }
            }
            return glyph;
        }

        static bool BitmapIsTransparent(BitmapExtended bitmap)
        {
            var pixels = bitmap.GetPixels();
            for (int y = 0; y < pixels.Height; y++)
            {
                for (int x = 0; x < pixels.Width; x++)
                {
                    if (pixels.GetPixel(x, y).A < 1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
