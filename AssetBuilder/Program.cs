using AtlasTexturePacker.Library;
using Game.Common;
using Game.Rendering;
using Game.Serialization;
using System;
using System.Collections.Generic;
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
            var fontCount = RenderFonts();
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
        private static int RenderFonts()
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

            var count = 0;

            var fonts = fontFiles.Select(item => FontLoader.Load(item)).ToList();
            foreach (var font in fonts)
            {
                if (font.Pages.Count > 1)
                {
                    throw new NotImplementedException("Multipage fonts are not supported. Try making the font page larger instead.");
                }

                var fontImagePath = Path.Combine(BuildFontsPath, font.Pages[0].File);
                DebugEx.Assert(File.Exists(fontImagePath));
                var page = new Bitmap(fontImagePath);

                foreach (var fontChar in font.Chars)
                {
                    var glyph = new BitmapExtended(fontChar.Width, fontChar.Height);
                    glyph.Name = count.ToString();
                    count++;
                    for (int y = 0; y < fontChar.Height; y++)
                    {
                        for (int x = 0; x < fontChar.Width; x++)
                        {
                            var color = page.GetPixel(fontChar.X + x, fontChar.Y + y);
                            glyph.SetPixel(x, y, color);
                        }
                    }
                    bitmaps.Add(glyph);
                }
            }

            bitmaps.AddRange(Directory
                .GetFiles(Path.Combine(AssetsPath, "Textures"))
                .Select(item => new BitmapExtended(item))
                .ToArray());
            var atlas = AtlasCreator.CreateAtlas("Atlas", bitmaps.ToArray());
            DebugEx.Assert(atlas.Length == 1);
            atlas[0].texture.Save(Path.Combine(BuildPath, "Atlas.png"), ImageFormat.Png);
        }
    }
}
