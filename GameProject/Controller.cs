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
using Game.Rendering;


namespace Game
{
    public class Controller : ITime, IController
    {
        public IInput Input { get; private set; }
        /// <summary>
        /// Keep pointless messages from the Poly2Tri library out of the console window.
        /// </summary>
        public static StreamWriter TrashLog = new StreamWriter(Stream.Null);
        public const int MicrosecondsInSecond = 1000000;
        public float TimeFixedStep = 0.0f;
        public Size CanvasSize { get; set; }
        public const int StepsPerSecond = 60;
        public const int DrawsPerSecond = 60;
        public const string TempLevelPrefix = "temp_level_";
        public int RenderCount = 0;
        public string[] ProgramArgs = new string[0];
        public SoundSystem SoundSystem { get; private set; }

        public static List<int> TextureGarbage = new List<int>();

        public static string FontFolder { get; private set; } = Path.Combine(new string[2] { "assets", "fonts" });
        public static string ShaderFolder { get; private set; } = Path.Combine(new string[2] { "assets", "shaders" });
        public static string TextureFolder { get; private set; } = Path.Combine(new string[2] { "assets", "textures" });
        public static string SoundFolder { get; private set; } = Path.Combine(new string[2] { "assets", "sounds" });

        /// <summary>
        /// Records time elapsed since the program start.
        /// </summary>
        Stopwatch _time = new Stopwatch();
        public double Time { get { return _time.ElapsedMilliseconds / (double)1000; } }
        /// <summary>
        /// The difference in seconds between the last OnUpdateEvent and the current OnRenderEvent.
        /// </summary>
        float _timeRenderDelta = 0.0f;
        public Renderer Renderer { get; private set; }
        public FontRenderer FontRenderer { get; private set; }
        public Font Default;
        public bool SoundEnabled { get; private set; }
        public readonly bool IsHeadless;

        public Controller()
        {
            IsHeadless = true;
        }

        public Controller(Window window)
            : this(window.ClientSize, window.InputExt)
        {
        }

        public Controller(Size canvasSize, Input input)
        {
            CanvasSize = canvasSize;
            Input = input;
            IsHeadless = false;
        }

        public virtual void OnLoad(EventArgs e)
        {
            _time.Start();

            if (!IsHeadless)
            {
                Renderer = new Renderer(CanvasSize);

                // Load textures from file
                Renderer.Textures.Add("default.png", new TextureFile(Path.Combine(TextureFolder, "default.png")));
                Renderer.Textures.Add("grid.png", new TextureFile(Path.Combine(TextureFolder, "grid.png")));
                Renderer.Textures.Add("lineBlur.png", new TextureFile(Path.Combine(TextureFolder, "lineBlur.png")));

                //Create the default font
                System.Drawing.Text.PrivateFontCollection privateFonts = new System.Drawing.Text.PrivateFontCollection();
                privateFonts.AddFontFile(Path.Combine(FontFolder, "times.ttf"));
                Default = new Font(privateFonts.Families[0], 14);
                FontRenderer = new FontRenderer(Default);

                // Load shaders from file
                Renderer.Shaders.Add("uber", new Shader(
                    Path.Combine(ShaderFolder, "vs_uber.glsl"),
                    Path.Combine(ShaderFolder, "fs_uber.glsl"),
                    true));

                SoundEnabled = false;
                if (SoundEnabled)
                {
                    SoundSystem = new SoundSystem();
                    SoundSystem.Initialize();
                    SoundSystem.Start();
                }
            }
            

            /*if (programArgs.Length == 1)
            {
                Serializer serializer = new Serializer();
                Scene scene = serializer.Deserialize(programArgs[0]);
                scene.SetActiveCamera(new Camera2(scene, new Transform2(new Vector2(), 10), CanvasSize.Width / (float)CanvasSize.Height));
                Renderer.AddLayer(scene);
                Portals.PortalCommon.UpdateWorldTransform(scene);
                if (programArgs[0].StartsWith(tempLevelPrefix))
                {
                    File.Delete(programArgs[0]);
                }
            }*/
        }

        public virtual void OnRenderFrame(FrameEventArgs e)
        {
            RenderCount++;
            _timeRenderDelta += (float)e.Time;
            Renderer.Render();
        }

        public virtual void OnUpdateFrame(FrameEventArgs e)
        {

            TimeFixedStep += MicrosecondsInSecond / (float)StepsPerSecond;
            _timeRenderDelta = 0;

            lock (Texture.LockDelete)
            {
                foreach (int iboElement in TextureGarbage.ToArray())
                {
                    int a = iboElement;
                    GL.DeleteTextures(1, ref a);
                }
                TextureGarbage.Clear();
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
                Renderer.SetCanvasSize(CanvasSize);
            }
        }
    }
}
