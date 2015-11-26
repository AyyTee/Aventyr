using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.ComponentModel;
using Cgen.Audio;
using System.Threading;
using System.Diagnostics;


namespace Game
{
    public class Controller
    {
        public Controller(Window window)
            : this(window.ClientSize, window.InputExt)
        {
        }

        public Controller(Size canvasSize, InputExt input)
        {
            CanvasSize = canvasSize;
            InputExt = input;
        }

        public InputExt InputExt;
        /// <summary>
        /// Intended to keep pointless messages from the Poly2Tri library out of the console window.
        /// </summary>
        public static StreamWriter Log = new StreamWriter("Triangulating.txt");
        public const int MICROSECONDS_IN_SECOND = 1000000;
        public float TimeFixedStep = 0.0f;
        public static Size CanvasSize;
        public const int StepsPerSecond = 60;
        public const int DrawsPerSecond = 60;
        public int RenderCount = 0;
        
        public static List<int> iboGarbage = new List<int>();

        public static String fontFolder = Path.Combine(new String[2] { "assets", "fonts" });
        public static String shaderFolder = Path.Combine(new String[2] { "assets", "shaders" });
        public static String textureFolder = Path.Combine(new String[2] { "assets", "textures" });
        public static String soundFolder = Path.Combine(new String[2] { "assets", "sounds" });
        
        /// <summary>
        /// Records time elapsed since the program start.
        /// </summary>
        public Stopwatch Time = new Stopwatch();
        /// <summary>
        /// The difference in seconds between the last OnUpdateEvent and the current OnRenderEvent.
        /// </summary>
        float TimeRenderDelta = 0.0f;
        public SoundSystem soundPlayer;
        public Renderer renderer;
        public FontRenderer FontRenderer;
        public Font Default;

        public virtual void OnLoad(EventArgs e)
        {
            Time.Start();
            Renderer.Init();
            renderer = new Renderer(this);

            // Load textures from file
            Renderer.Textures.Add("default.png", Renderer.LoadImage(Path.Combine(textureFolder, "default.png")));
            Renderer.Textures.Add("grid.png", Renderer.LoadImage(Path.Combine(textureFolder, "grid.png")));

            //Create the default font
            System.Drawing.Text.PrivateFontCollection privateFonts = new System.Drawing.Text.PrivateFontCollection();
            privateFonts.AddFontFile(Path.Combine(fontFolder, "times.ttf"));
            Default = new Font(privateFonts.Families[0], 14);
            FontRenderer = new FontRenderer(Default);

            // Load shaders from file
            Renderer.Shaders.Add("default", new ShaderProgram(Path.Combine(shaderFolder, "vs.glsl"), Path.Combine(shaderFolder, "fs.glsl"), true));
            Renderer.Shaders.Add("textured", new ShaderProgram(Path.Combine(shaderFolder, "vs_tex.glsl"), Path.Combine(shaderFolder, "fs_tex.glsl"), true));
            Renderer.Shaders.Add("text", new ShaderProgram(Path.Combine(shaderFolder, "vs_text.glsl"), Path.Combine(shaderFolder, "fs_text.glsl"), true));
        }

        public virtual void OnRenderFrame(FrameEventArgs e)
        {
            RenderCount++;
            TimeRenderDelta += (float)e.Time;
            renderer.Render();
        }

        public virtual void OnUpdateFrame(FrameEventArgs e)
        {
            TimeFixedStep += MICROSECONDS_IN_SECOND / (float)StepsPerSecond;
            TimeRenderDelta = 0;
            //get rid of all ibo elements no longer used
            lock ("delete")
            {
                foreach (int iboElement in iboGarbage.ToArray())
                {
                    int a = iboElement;
                    GL.DeleteBuffers(1, ref a);
                }
                iboGarbage.Clear();
            }
        }

        public virtual void OnClosing(CancelEventArgs e)
        {
            if (soundPlayer != null)
            {
                soundPlayer.Dispose();
            }
        }

        public virtual void OnResize(EventArgs e, Size canvasSize)
        {
            CanvasSize = canvasSize;
        }
    }
}
