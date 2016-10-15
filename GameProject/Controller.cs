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
    public class Controller : ITime
    {
        public InputExt InputExt;
        /// <summary>
        /// Keep pointless messages from the Poly2Tri library out of the console window.
        /// </summary>
        public static StreamWriter TrashLog = new StreamWriter(Stream.Null);
        public const int MICROSECONDS_IN_SECOND = 1000000;
        public float TimeFixedStep = 0.0f;
        public static Size CanvasSize;
        public const int StepsPerSecond = 60;
        public const int DrawsPerSecond = 60;
        public const string tempLevelPrefix = "temp_level_";
        public int RenderCount = 0;
        public string[] programArgs = new string[0];

        public static List<int> iboGarbage = new List<int>();
        public static List<int> textureGarbage = new List<int>();

        public static string fontFolder = Path.Combine(new string[2] { "assets", "fonts" });
        public static string shaderFolder = Path.Combine(new string[2] { "assets", "shaders" });
        public static string textureFolder = Path.Combine(new string[2] { "assets", "textures" });
        public static string soundFolder = Path.Combine(new string[2] { "assets", "sounds" });

        /// <summary>
        /// Records time elapsed since the program start.
        /// </summary>
        Stopwatch _time = new Stopwatch();
        public double Time { get { return _time.ElapsedMilliseconds / (double)1000; } }
        /// <summary>
        /// The difference in seconds between the last OnUpdateEvent and the current OnRenderEvent.
        /// </summary>
        float TimeRenderDelta = 0.0f;
        public Renderer renderer;
        public FontRenderer FontRenderer;
        public Font Default;
        public bool SoundEnabled { get; private set; }

        public Controller(Window window, string[] args)
            : this(window.ClientSize, window.InputExt)
        {
            programArgs = args;
            Debug.Assert(programArgs.Length <= 1);
        }

        public Controller(Size canvasSize, InputExt input)
        {
            CanvasSize = canvasSize;
            InputExt = input;
        }

        public virtual void OnLoad(EventArgs e)
        {
            _time.Start();

            renderer = new Renderer();

            SoundEnabled = false;
            if (SoundEnabled)
            {
                SoundSystem.Instance.Initialize();
                SoundSystem.Instance.Start();
            }

            // Load textures from file
            Renderer.Textures.Add("default.png", new TextureFile(Path.Combine(textureFolder, "default.png")));
            Renderer.Textures.Add("grid.png", new TextureFile(Path.Combine(textureFolder, "grid.png")));
            Renderer.Textures.Add("lineBlur.png", new TextureFile(Path.Combine(textureFolder, "lineBlur.png")));

            //Create the default font
            System.Drawing.Text.PrivateFontCollection privateFonts = new System.Drawing.Text.PrivateFontCollection();
            privateFonts.AddFontFile(Path.Combine(fontFolder, "times.ttf"));
            Default = new Font(privateFonts.Families[0], 14);
            FontRenderer = new FontRenderer(Default);

            // Load shaders from file
            Renderer.Shaders.Add("uber", new ShaderProgram(
                Path.Combine(shaderFolder, "vs_uber.glsl"),
                Path.Combine(shaderFolder, "fs_uber.glsl"),
                true));

            if (programArgs.Length == 1)
            {
                Serializer serializer = new Serializer();
                Scene scene = serializer.Deserialize(programArgs[0]);
                scene.SetActiveCamera(new Camera2(scene, new Transform2(new Vector2(), 10), CanvasSize.Width / (float)CanvasSize.Height));
                renderer.AddLayer(scene);
                if (programArgs[0].StartsWith(tempLevelPrefix))
                {
                    File.Delete(programArgs[0]);
                }
            }
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
            lock (Model.LockDelete)
            {
                foreach (int iboElement in iboGarbage.ToArray())
                {
                    int a = iboElement;
                    GL.DeleteBuffers(1, ref a);
                }
                iboGarbage.Clear();
            }
            lock (Texture.LockDelete)
            {
                foreach (int iboElement in textureGarbage.ToArray())
                {
                    int a = iboElement;
                    GL.DeleteTextures(1, ref a);
                }
                textureGarbage.Clear();
            }
        }

        public virtual void OnClosing(CancelEventArgs e)
        {
            if (SoundEnabled)
            {
                SoundSystem.Instance.Stop();
            }
        }

        public virtual void OnResize(EventArgs e, Size canvasSize)
        {
            if (canvasSize.Width > 0 && canvasSize.Height > 0)
            {
                CanvasSize = canvasSize;
            }
        }
    }
}
