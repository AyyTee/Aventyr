using AtlasTexturePacker.Library;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        static int BytesInKiloByte => 2 << 10;

        public static string RootPath => Path.Combine(Environment.CurrentDirectory, "../../..");
        public static string ToolsPath => Path.Combine(RootPath, "Tools");
        public static string BuildPath => Path.Combine(RootPath, "Build");
        public static string AssetsPath => Path.Combine(RootPath, "Assets");

        public static string BlenderPath => Path.Combine(ToolsPath, "Blender");
        public static string BlenderZipPath => Path.Combine(ToolsPath, "Blender.zip");
        public static string BlenderExePath => Path.Combine(BlenderPath, Directory.GetDirectories(BlenderPath).First(), "blender.exe");

        public static (long ReceivedBytes, long TotalBytes) DownloadProgress = (0, 0);

        public static void Main(string[] args)
        {
            var rootDirectory = new DirectoryInfo(RootPath);
            if (!rootDirectory.GetDirectories().Any(item => item.Name == "Assets"))
            {
                throw new DirectoryNotFoundException("Assets folder is missing from root directory.");
            }

            if (Directory.Exists(BuildPath))
            {
                Directory.Delete(BuildPath, true);
            }
            Directory.CreateDirectory(BuildPath);
            Directory.CreateDirectory(ToolsPath);
            Directory.CreateDirectory(BlenderPath);

            FetchTools();

            CommandLine(AssetsPath, $"{BlenderExePath} Models.blend --background --python BatchExport.py");

            Console.WriteLine("Build complete!");
            Console.ReadLine();
            //var bitmap = new BitmapExtended();
            //AtlasCreator.CreateAtlas("TexturePage", );
        }

        public static void FetchTools()
        {
            if (!Directory.GetDirectories(BlenderPath).Any())
            {
                if (!File.Exists(BlenderZipPath))
                {
                    Console.WriteLine("Downloading tools.  This could take several minutes...");
                    Download();
                }

                try
                {
                    ZipFile.ExtractToDirectory(BlenderZipPath, BlenderPath);
                }
                catch (InvalidDataException)
                {
                    Console.WriteLine("Zip file was corrupted.  Trying to download again...");
                    Download();
                    ZipFile.ExtractToDirectory(BlenderZipPath, BlenderPath);
                }
            }
        }

        static void Download()
        {
            using (var client = new WebClient())
            {
                client.DownloadProgressChanged += (_, e) => DownloadProgress = (e.BytesReceived, e.TotalBytesToReceive);
                client.DownloadFileAsync(new Uri("http://download.blender.org/release/Blender2.70/blender-2.70a-windows32.zip"), BlenderZipPath);

                while (client.IsBusy)
                {
                    if (DownloadProgress.TotalBytes > 0)
                    {
                        var percentage = (100 * DownloadProgress.ReceivedBytes) / DownloadProgress.TotalBytes;
                        Console.WriteLine($"{percentage}% {DownloadProgress.ReceivedBytes / BytesInKiloByte}/{DownloadProgress.TotalBytes / BytesInKiloByte} kB remaining");
                    }
                    Thread.Sleep(2000);
                }
            }
        }

        /// <remarks>Original code found here: https://stackoverflow.com/a/32872174 </remarks>
        public static void CommandLine(string workingDirectory, string command)
        {
            var cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.WorkingDirectory = workingDirectory;
            cmd.Start();

            cmd.StandardInput.WriteLine(command);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();
            Console.WriteLine(cmd.StandardOutput.ReadToEnd());
        }
    }
}
